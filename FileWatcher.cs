using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DumpLoader
{
    public class FileWatcher
    {
        private ServerOptions _options;

        public FileWatcher(ServerOptions options)
        {
            try
            {
                this._options = options;
                LogConsole.LogInformation($"Create directories in [{Directory.GetCurrentDirectory()}]", null);

                List<string> directories = new List<string>() {"done", "error", "watch"};

                foreach (string path in directories)
                {
                    Directory.CreateDirectory(path);
                    LogConsole.LogInformation($"Directory [{path}]...", "OK");
                }

                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = Path.Combine(Directory.GetCurrentDirectory(), "watch");
                watcher.Filter = "*.bak";
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(FileChanged);
                watcher.EnableRaisingEvents = true;

                LogConsole.LogInformation($"Starting watcher in [{watcher.Path} ({watcher.Filter})]...", "OK");

                Console.ReadKey(false);
                LogConsole.LogInformation($"Stopping watcher...", "OK");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                LogConsole.LogError(ex);
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            var tokenSource = new CancellationTokenSource();

            try
            {
                if(!FileReady(e.FullPath)) return;

                LogConsole.LogInformation("Importing...", Path.GetFileNameWithoutExtension(e.FullPath));
        
                var taskLoading = Task.Run(() => {
                    while(!tokenSource.IsCancellationRequested) {
                        Console.Write(".");
                        Thread.Sleep(2000);
                    }
                }, tokenSource.Token);

                var restoreDumpTask = Task.Run(() => { new ImportDump(this._options, e.FullPath); });
                restoreDumpTask.Wait();
                tokenSource.Cancel();
                MoveFile(e.FullPath, "done");

                LogConsole.LogInformation($"Dump {Path.GetFileNameWithoutExtension(e.FullPath)}...", "OK");                
            }
            catch (Exception ex)
            {
                tokenSource.Cancel();
                MoveFile(e.FullPath, "error", reason: ex.Message);
                LogConsole.LogError(ex);
            }
        }

        private void MoveFile(string source, string destination, string reason = null)
        {
            var newPath = Path.Combine(Directory.GetCurrentDirectory(), destination);
            File.Move(source, Path.Combine(newPath, Path.GetFileName(source)), true);
            if (!string.IsNullOrEmpty(reason)) File.WriteAllText(Path.Combine(newPath, Path.GetFileNameWithoutExtension(source) + ".txt"), reason);
        }

        private bool FileReady(string path)
        {
            try
            {
                using(var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}