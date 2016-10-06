using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using NLog;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    class ArchiveProject : IProject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string[] Extensions = { ".zip", ".owr" };
        private readonly string _tempDirectory;

        public string Name { get; }
        public string ProjectPath { get; }
        public bool IsArchive => true;
        public ZipFile Archive { get; private set; }
        public string IndexFile { get; } = "index.html";
        public HtmlFileIndex SearchIndex { get; private set; }
        public string IndexUrl => GetUrl(IndexFile);

        public ArchiveProject(string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            ProjectPath = path;
            _tempDirectory = Path.Combine(Path.GetTempPath(), "OfflineWebsiteViewer", Name);
        }

        public bool Open()
        {
            try
            {
                Logger.Info($"Opening '{ProjectPath}' archive project");
                Archive = new ZipFile(ProjectPath);

                Logger.Trace($"Extracting search index from '{ProjectPath}' to '{_tempDirectory}'");
                var fastZip = new FastZip();
                fastZip.ExtractZip(ProjectPath, _tempDirectory, "SearchIndex/*");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to open '{ProjectPath}'");
                return false;
                throw;
            }

            SearchIndex = new HtmlFileIndex(Path.Combine(_tempDirectory, "SearchIndex"));
            return true;
        }

        public string GetUrl(string file)
        {
            return $"zip://archive/{file}";
        }

        public void CreateIndex(Action callback)
        {
        }

        public void OpenFile(string file)
        {
            if (file.StartsWith("/"))
                file = file.Substring(1, file.Length - 1);

            Logger.Trace($"Extracting '{file}' from '{ProjectPath}' to '{_tempDirectory}'");
            var fastZip = new FastZip();
            fastZip.ExtractZip(ProjectPath, _tempDirectory, file);
            Process.Start(Path.Combine(_tempDirectory, file));
        }

        public void Dispose()
        {
            Logger.Trace($"Closing archive");
            Archive.Close();

            try
            {
                Logger.Trace($"Deleting temp directory: {_tempDirectory}");
                Directory.Delete(_tempDirectory, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to delete temp directory: {_tempDirectory}");
            }
        }
    }
}