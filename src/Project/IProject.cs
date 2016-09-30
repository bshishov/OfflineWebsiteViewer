using System;
using OfflineWebsiteViewer.Search;

namespace OfflineWebsiteViewer.Project
{
    public interface IProject : IDisposable
    {
        //string Path { get; }
        string IndexFile { get; }
        string IndexUrl { get; }
        HtmlFileIndex SearchIndex { get; }
        string GetUrl(string file);
        void CreateIndex(Action callback);
    }
}
