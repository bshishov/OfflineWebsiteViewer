using System;
using System.Diagnostics;
using System.IO;
using NLog;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    class FlatDirectoryProject : IProject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Name { get; }
        public bool IsArchive => false;
        public string IndexFile { get; } = "index.html";
        public string IndexUrl => GetUrl(IndexFile);
        public HtmlFileIndex SearchIndex { get; private set; }
        public string ProjectPath => _directoryPath;

        private readonly string _directoryPath;

        public FlatDirectoryProject(string path)
        {
            Name = new DirectoryInfo(path).Name;
            _directoryPath = path;
        }

        public bool Open()
        {
            Logger.Info($"Opening '{ProjectPath}' folder project");
            SearchIndex = new HtmlFileIndex(Path.Combine(_directoryPath, "SearchIndex"));
            return true;
        }

        public string GetUrl(string file)
        {
            return $"file://{Path.Combine(_directoryPath, file)}";
        }

        public void CreateIndex(Action callback = null)
        {
            if (!SearchIndex.IsEmptyIndex)
                SearchIndex.ClearIndex();
            
            SearchIndex.RunReindexTask(_directoryPath, callback);
        }

        public void OpenFile(string file)
        {
            Process.Start(Path.Combine(_directoryPath, file));
        }

        public void Dispose()
        {
        }
    }
}