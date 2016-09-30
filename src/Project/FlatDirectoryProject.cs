using System;
using System.IO;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    class FlatDirectoryProject : IProject
    {
        public string IndexFile { get; } = "index.html";
        public string IndexUrl => GetUrl(IndexFile);
        public HtmlFileIndex SearchIndex { get; }

        private readonly string _directoryPath;

        public FlatDirectoryProject(string path)
        {
            _directoryPath = path;
            SearchIndex = new HtmlFileIndex(Path.Combine(_directoryPath, "SearchIndex"));
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

        public void Dispose()
        {
        }
    }
}