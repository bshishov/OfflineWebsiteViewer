using System;
using System.Windows;
using CefSharp;
using NLog;

namespace OfflineWebsiteViewer.Browser
{
    public class CustomDownloadHandler : IDownloadHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    var project = (Application.Current as App)?.Project;
                    if(project != null)
                    { 
                        try
                        {
                            var fileUri = new Uri(downloadItem.Url);
                            Logger.Info($"Trying to opening file {fileUri.LocalPath}");
                            project.OpenFile(fileUri.LocalPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "Failed to open file directly, falling back to show save dialog");
                            callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                        }
                    }
                    else
                    {
                        callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                    }
                }
            }
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
        }
    }
}
