using System;
using System.Buffers;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ZipToolKit
{
    internal static class ZipTool
    {
        /// <summary>
        /// ランダム生成機
        /// </summary>
        private static Random Random { get; } = new Random();

        /// <summary>
        /// Zipファイルに使うランダムなパスワードを作成する
        /// </summary>
        /// <param name="length">パスワードの文字数</param>
        /// <param name="passwordChars">パスワードに使う1バイト文字の並び</param>
        /// <returns>パスワード</returns>
        public static string GenerateZipPassword(int length, string passwordChars)
        {
            //zipとファイル名に使えるキャラクタセット
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                //ランダムな文字を選択
                char c = passwordChars[Random.Next(passwordChars.Length)];
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// フォルダを圧縮する
        /// </summary>
        /// <param name="zipStream">圧縮先のZipStream</param>
        /// <param name="folderName">圧縮するフォルダのフルパス</param>
        /// <param name="offsetFolderName">圧縮時のルートフォルダのフルパス</param>
        public static void CompressFolder(ZipOutputStream zipStream, string folderName, string offsetFolderName)
        {
            //フォルダの中にあるファイルを圧縮
            foreach (var file in Directory.GetFiles(folderName))
            {
                CompressFile(zipStream, file, offsetFolderName);
            }

            //子フォルダを再帰的に圧縮
            foreach (var folder in Directory.GetDirectories(folderName))
            {
                CompressFolder(zipStream, folder, offsetFolderName);
            }
        }

        /// <summary>
        /// ファイルを圧縮
        /// </summary>
        /// <param name="zipStream">圧縮先のZipStream</param>
        /// <param name="filename">ファイル名フルパス</param>
        /// <param name="offsetFolderName">圧縮時のルートフォルダのフルパス</param>
        public static void CompressFile(ZipOutputStream zipStream, string filename, string offsetFolderName)
        {
            //フォルダのオフセット値を取得
            var folderOffset = offsetFolderName.Length + (offsetFolderName.EndsWith("\\") ? 0 : 1);

            //ファイル名の余計なパスを消す
            string entryName = filename.Substring(folderOffset);
            entryName = ZipEntry.CleanName(entryName);

            //圧縮するファイルを表示←非常に良くない
            Console.WriteLine(entryName);

            //ファイル情報書き込み
            var fi = new FileInfo(filename);
            var newEntry = new ZipEntry(entryName)
            {
                DateTime = fi.LastWriteTime,
                Size = fi.Length,
            };
            zipStream.PutNextEntry(newEntry);
            try
            {
                var buffer = ArrayPool<byte>.Shared.Rent(4096);
                try
                {
                    //ファイル内容書き込み
                    using (FileStream sr = File.OpenRead(filename))
                    {
                        StreamUtils.Copy(sr, zipStream, buffer);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            finally
            {
                zipStream.CloseEntry();
            }
        }
    }
}
