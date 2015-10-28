using System;
using System.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ZipToolKit;

namespace PassZipper
{
    /// <summary>
    /// define的なクラス
    /// </summary>
    static class Common
    {
        /// <summary>
        /// パスワードに使用できる文字
        /// </summary>
        static public readonly string PasswordChars = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_+-=";
        
        /// <summary>
        /// デフォルトのZipファイル出力先
        /// </summary>
        static public readonly string DefaultPath = ".";

        /// <summary>
        /// 設定ファイル名
        /// </summary>
        static public readonly string SettingFileName = "setting.ini";

        /// <summary>
        /// パスワードの文字数
        /// </summary>
        static public readonly int PasswordLength = 8;

    }

    /// <summary>
    /// プログラムのメインクラス
    /// </summary>
    class Program
    {
        
        static private string GetOutputPath() {
            if (!File.Exists(Common.SettingFileName))
            {
                File.WriteAllText(Common.SettingFileName, Common.DefaultPath);
            }
            return File.ReadLines(Common.SettingFileName).First();
        }

        /// <summary>
        /// メイン関数
        /// </summary>
        /// <param name="args">
        /// <para>圧縮ファイルのパス</para>
        /// <para>フォルダや、複数ファイルにも対応</para>
        /// </param>
        static void Main(string[] args)
        {
            //有効なパスが一つもなければ終了
            if (!args.Any(s => Directory.Exists(s) || File.Exists(s)))
            {
                return;
            }

            Console.WriteLine("Zip files");
            args.ToList().ForEach(x => Console.WriteLine(x));

            //パスワード生成
            var passWord = ZipTool.GenerateZipPassword(Common.PasswordLength, Common.PasswordChars);
            Console.WriteLine("password : " + passWord);

            //出力先のZipファイルを作成
            var outputPath = GetOutputPath();
            var outputFilename = outputPath + "\\output.zip";
            File.WriteAllText(outputPath + "\\passwd.txt", passWord);
            using (var fsOut = File.Create(outputFilename))
            {
                var zipStream = new ZipOutputStream(fsOut)
                {
                    Password = passWord,
                };

                //フォルダ群の圧縮
                var folders = args.Where(name => Directory.Exists(name)).ToList();
                folders.ForEach(folder =>
                {
                    ZipTool.CompressFolder(folder, Directory.GetParent(folder).FullName, zipStream);
                });

                //ファイル群の圧縮
                var files = args.Where(name => File.Exists(name)).ToList();
                files.ForEach(file =>
                {
                    ZipTool.CompressFile(file, new FileInfo(file).DirectoryName, zipStream);
                });
                
                //ファイル閉じる
                zipStream.IsStreamOwner = true;
                zipStream.Close();
            }
        }
    }
}
