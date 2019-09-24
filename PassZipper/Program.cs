using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ZipToolKit;

namespace PassZipper
{
    /// <summary>
    /// プログラムのメインクラス
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// メイン関数
        /// </summary>
        /// <param name="args">
        /// <para>圧縮ファイルのパス</para>
        /// <para>フォルダや、複数ファイルにも対応</para>
        /// </param>
        private static async Task Main(string[] args)
        {
            //有効なパスが一つもなければ終了
            if (!args.Any(s => Directory.Exists(s) || File.Exists(s)))
            {
                return;
            }
            Console.WriteLine("application path  : " + Common.ApplicationDirPath);

            //Console.WriteLine("Zip files");
            foreach (var x in args)
            {
                Console.WriteLine(x);
            }

            //パスワード生成
            var passWord = ZipTool.GenerateZipPassword(Common.PasswordLength, Common.PasswordChars);
            Console.WriteLine("password : " + passWord);

            // パスワードファイル作成
            using (var templateStream = new StreamReader(Common.TemplateFilePath))
            using (var passwordFile = new StreamWriter(Path.Combine(GetOutputDirName(args), Common.PassWordFileName)))
            {
                var passwordFileString = (await templateStream.ReadToEndAsync())
                    .Replace(Common.PasswordKeyWord, passWord);
                await passwordFile.WriteAsync(passwordFileString);
            }

            //出力先のZipファイルを作成
            var outputFileName = Path.Combine(GetOutputDirName(args), Common.ZipFileName);

            using (FileStream fsOut = File.Create(outputFileName))
            using (var zipStream = new ZipOutputStream(fsOut)
            {
                Password = passWord,
            })
            {
                //フォルダ群の圧縮
                foreach (var folder in args.Where(name => Directory.Exists(name)))
                {
                    ZipTool.CompressFolder(zipStream, folder, Directory.GetParent(folder).FullName);
                }

                //ファイル群の圧縮
                foreach (var file in args.Where(name => File.Exists(name)))
                {
                    ZipTool.CompressFile(zipStream, file, new FileInfo(file).DirectoryName);
                }
            }
        }

        /// <summary>
        /// 出力フォルダ設定
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetOutputDirName(string[] args)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
    }
}
