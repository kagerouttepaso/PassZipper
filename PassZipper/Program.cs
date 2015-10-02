using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace PassZipper
{
    class Program
    {
        static readonly string PasswordChars = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~@()_+-=";
        static readonly string DefaultPath = ".";
        static readonly string SettingFileName = "setting.ini";

        /// <summary>
        /// ランダム生成器
        /// </summary>
        static Random random = new Random();

        /// <summary>
        /// Zipファイルに使うランダムなパスワードを作成する
        /// </summary>
        /// <param name="length">パスワードの文字数</param>
        /// <returns></returns>
        static private string GenerateZipPassword(int length)
        {
            //zipとファイル名に使えるキャラクタセット
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                //ランダムな文字を選択
                char c = PasswordChars[random.Next(PasswordChars.Length)];
                sb.Append(c);
            }

            return sb.ToString();
        }

        static private void CompressFolder(string folderName, ZipOutputStream zipStream, string offsetFolderName)
        {
            //フォルダのオフセット値を取得
            var folderOffset = offsetFolderName.Length + (offsetFolderName.EndsWith("\\") ? 0 : 1);

            //圧縮するファイルを取得
            var files = Directory.GetFiles(folderName);
            files.ToList().ForEach(filename =>
            {
                CompressFile(filename, zipStream, offsetFolderName);
            });
            //子フォルダを再帰的にzip
            var folders = Directory.GetDirectories(folderName);
            folders.ToList().ForEach(folder =>
            {
                CompressFolder(folder, zipStream, offsetFolderName);
            });
        }

        static private void CompressFile(string filename, ZipOutputStream zipStream, string offsetFolderName)
        {
            //フォルダのオフセット値を取得
            var folderOffset = offsetFolderName.Length + (offsetFolderName.EndsWith("\\") ? 0 : 1);

            var fi = new FileInfo(filename);

            //ファイル名の余計なパスを消す
            string entryName = filename.Substring(folderOffset);
            entryName = ZipEntry.CleanName(entryName);
            Console.WriteLine(entryName);

            //ファイル情報書き込み
            var newEntry = new ZipEntry(entryName)
            {
                DateTime = fi.LastWriteTime,
                Size = fi.Length,
            };
            zipStream.PutNextEntry(newEntry);

            //ファイル書き込み
            var buffer = new byte[4096];
            using (var streamReader = File.OpenRead(filename))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            zipStream.CloseEntry();
        }

        static private string GetOutputPath() {
            if (!File.Exists(SettingFileName))
            {
                File.WriteAllText(SettingFileName, DefaultPath);
            }
            return File.ReadLines(SettingFileName).First();
        }

        /// <summary>
        /// メイン関数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (!args.Any(s => Directory.Exists(s) || File.Exists(s)))
            {
                return;
            }
            Console.WriteLine("Zip files");
            args.ToList().ForEach(x => Console.WriteLine(x));

            var passWord = GenerateZipPassword(20);
            Console.WriteLine("password : " + passWord);

            //Create
            var outputPath = GetOutputPath();
            var outputFilename = outputPath + "\\output pass=" + passWord + ".zip";
            File.WriteAllText(outputPath + "\\passwd pass=" + passWord + ".txt", passWord);

            using (var fsOut = File.Create(outputFilename))
            {
                var zipStream = new ZipOutputStream(fsOut)
                {
                    Password = passWord,
                };

                var folders = args.Where(name => Directory.Exists(name)).ToList();
                folders.ForEach(folder =>
                {
                    CompressFolder(folder, zipStream, Directory.GetParent(folder).FullName);
                });
                var files = args.Where(name => File.Exists(name)).ToList();
                files.ForEach(file =>
                {
                    CompressFile(file, zipStream, new FileInfo(file).DirectoryName);
                });
                
                zipStream.IsStreamOwner = true;
                zipStream.Close();
            }
        }
    }
}
