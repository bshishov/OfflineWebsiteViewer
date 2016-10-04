using System;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    public interface IProject : IDisposable
    {
        string Name { get; }
        string ProjectPath { get; }
        bool IsArchive { get; }
        string IndexFile { get; }
        string IndexUrl { get; }
        HtmlFileIndex SearchIndex { get; }
        bool Open();
        string GetUrl(string file);
        void CreateIndex(Action callback);
        void OpenFile(string file);
    }
}
