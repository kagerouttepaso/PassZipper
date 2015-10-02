using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Concurrent;

namespace PassZipper
{
    public partial class Form1 : Form
    {
        public List<string> StartArgs { get; set; }
        public Form1()
        {
            StartArgs = new List<string>();
            InitializeComponent();
        }

        private void Compless(List<string> args)
        {
            Func<string, int> fileCounter = x => 0;
            int nowCount = 0;
            fileCounter = name =>
            {
                if (File.Exists(name))
                {
                    Interlocked.Increment(ref nowCount);
                    return 1;
                }
                else if (Directory.Exists(name))
                {
                    var count = Directory.GetFiles(name).Count();
                    Interlocked.Add(ref nowCount, count);
                    count += Directory.GetDirectories(name).AsParallel().Sum(dirName => fileCounter(dirName));
                    return count;
                }
                else
                {
                    return 0;
                }
            };
            Task.Run(() => {
                args.ForEach(x =>
                {
                    fileCounter(x);
                    StatusLabel.Text = nowCount.ToString();
                });
            });
            
            var passWord = Program.GenerateZipPassword(20);

            //Create
            var outputPath = Program.GetOutputPath();
            var outputFilename = outputPath + "\\output pass=" + passWord + ".zip";

            using (var fsOut = File.Create(outputFilename))
            {
                var zipStream = new ZipOutputStream(fsOut)
                {
                    Password = passWord,
                };

                var folders = args.Where(name => Directory.Exists(name)).ToList();
                folders.ForEach(folder =>
                {
                    Program.CompressFolder(folder, zipStream, Directory.GetParent(folder).FullName);
                });
                var files = args.Where(name => File.Exists(name)).ToList();
                files.ForEach(file =>
                {
                    Program.CompressFile(file, zipStream, new FileInfo(file).DirectoryName);
                });

                zipStream.IsStreamOwner = true;
                zipStream.Close();
                this.Close();
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;   
            }
            StartArgs = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));
            Compless(StartArgs);

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

    }
}
