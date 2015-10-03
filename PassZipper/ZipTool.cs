using System;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ZipToolKit
{
    static class ZipTool
    {

        /// <summary>
        /// ランダム生成機
        /// </summary>
        static private Random random = new Random();

        /// <summary>
        /// Zipファイルに使うランダムなパスワードを作成する
        /// </summary>
        /// <param name="length">パスワードの文字数</param>
        /// <param name="passwordChars">パスワードに使う1バイト文字の並び</param>
        /// <returns>パスワード</returns>
        static public string GenerateZipPassword(int length,string passwordChars)
        {
            //zipとファイル名に使えるキャラクタセット
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                //ランダムな文字を選択
                char c = passwordChars[random.Next(passwordChars.Length)];
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// フォルダを圧縮する
        /// </summary>
        /// <param name="folderName">圧縮するフォルダのフルパス</param>
        /// <param name="offsetFolderName">圧縮時のルートフォルダのフルパス</param>
        /// <param name="zipStream">圧縮先のZipStream</param>
        static public void CompressFolder(string folderName, string offsetFolderName, ZipOutputStream zipStream)
        {
            //フォルダのオフセット値を取得
            var folderOffset = offsetFolderName.Length + (offsetFolderName.EndsWith("\\") ? 0 : 1);

            //フォルダの中にあるファイルを圧縮
            var files = Directory.GetFiles(folderName);
            files.ToList().ForEach(filename =>
            {
                CompressFile(filename, offsetFolderName, zipStream);
            });

            //子フォルダを再帰的に圧縮
            var folders = Directory.GetDirectories(folderName);
            folders.ToList().ForEach(folder =>
            {
                CompressFolder(folder, offsetFolderName, zipStream);
            });
        }

        /// <summary>
        /// ファイルを圧縮
        /// </summary>
        /// <param name="filename">ファイル名フルパス</param>
        /// <param name="offsetFolderName">圧縮時のルートフォルダのフルパス</param>
        /// <param name="zipStream">圧縮先のZipStream</param>
        static public void CompressFile(string filename, string offsetFolderName, ZipOutputStream zipStream)
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

            //ファイル内容書き込み
            var buffer = new byte[4096];
            using (var streamReader = File.OpenRead(filename))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }

            zipStream.CloseEntry();
        }
    }
}
