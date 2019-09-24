using System.IO;
using System.Reflection;

namespace PassZipper
{
    /// <summary>
    /// define的なクラス
    /// </summary>
    internal static class Common
    {
        /// <summary>
        /// パスワードに使用できる文字
        /// </summary>
        public static string PasswordChars { get; } = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_+-=";

        /// <summary>
        /// パスワードの文字数
        /// </summary>
        public static int PasswordLength { get; } = 12;

        /// <summary>
        /// アプリケーションの格納フォルダ
        /// </summary>
        public static string ApplicationDirPath { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

        public static string TemplateFilePath => ApplicationDirPath + @"\template.txt";

        /// <summary>
        /// ZIPファイル名
        /// </summary>
        public static string ZipFileName { get; } = "archive.zi_";

        /// <summary>
        /// パスワード名
        /// </summarye
        public static string PassWordFileName { get; } = "password.txt";

        public static string PasswordKeyWord { get; } = "%PASSWORD%";
    }
}
