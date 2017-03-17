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
        static public string PasswordChars = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_+-=";

        /// <summary>
        /// パスワードの文字数
        /// </summary>
        static public readonly int PasswordLength = 8;
        
        /// <summary>
        /// アプリケーションの格納フォルダ
        /// </summary>
        static public readonly string ApplicationPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
        /// <summary>
        /// 設定ファイルのフルパス
        /// </summary>
        static public readonly string SettingFilePath = ApplicationPath + @"\setting.txt";
        static public readonly string TemplateFilePath = ApplicationPath + @"\template.txt";

        /// <summary>
        /// ZIPファイル名
        /// </summary>
        static public readonly string ZipFileName = "archive.zi_";

        /// <summary>
        /// パスワード名
        /// </summary>
        static public readonly string PassWordFileName = "password.txt";

        static public readonly string PasswordKeyWord = "%PASSWORD%";
    }

    /// <summary>
    /// プログラムのメインクラス
    /// </summary>
    class Program
    {
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
            Console.WriteLine("application path  : " + Common.ApplicationPath);
            Console.WriteLine("setting file path : " + Common.SettingFilePath);

            //Console.WriteLine("Zip files");
            args.ToList().ForEach(x => Console.WriteLine(x));

            //パスワード生成
            var passWord = ZipTool.GenerateZipPassword(Common.PasswordLength, Common.PasswordChars);
            Console.WriteLine("password : " + passWord);

            //出力先のZipファイルを作成
            var outputDirName = GetOutputDirName(args);
            var outputFileName = outputDirName + @"\" + Common.ZipFileName;

            string passwordFileString = new StreamReader(Common.TemplateFilePath).ReadToEnd()
                .Replace(Common.PasswordKeyWord, passWord);
            File.WriteAllText(outputDirName + @"\" + Common.PassWordFileName, passwordFileString);

            using (var fsOut = File.Create(outputFileName))
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

        /// <summary>
        /// 出力フォルダ設定
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetOutputDirName(string[] args)
        {
            // ファイルなかったら作る
            if (!File.Exists(Common.SettingFilePath))
            {
                using (var writer = new StreamWriter(Common.ApplicationPath))
                {
                    writer.WriteLine(Common.SettingFilePath);
                }
            }
            string ret = "";
            using (var reader = new StreamReader(Common.SettingFilePath))
            {
                ret = reader.ReadLine();
            }
            return ret;
        }
    }
}
