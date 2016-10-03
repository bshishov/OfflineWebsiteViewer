using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    class ArchiveProject : IProject
    {
        public const string Extension = ".zip";

        public string Name { get; }
        public string ProjectPath { get; }
        public bool IsArchive => true;
        public ZipFile Archive { get; }
        public string IndexFile { get; } = "index.html";
        public HtmlFileIndex SearchIndex { get; }
        public string IndexUrl => GetUrl(IndexFile);

        public ArchiveProject(string path)
        {
            Name = new FileInfo(path).Name;
            Archive = new ZipFile(path);
            ProjectPath = path;

            //var searchIndexEntry = Archive.GetEntry("SearchEntry");
            var searchIndexDirectory = Path.Combine(Path.GetTempPath(), "SearchIndex");

            var fastZip = new FastZip();
            fastZip.ExtractZip(path, searchIndexDirectory, "SearchIndex/*");
            SearchIndex = new HtmlFileIndex(searchIndexDirectory);
        }

        public string GetUrl(string file)
        {
            return $"zip://archive/{file}";
        }

        public void CreateIndex(Action callback)
        {
        }

        public void Dispose()
        {
            Archive.Close();
        }
    }
}