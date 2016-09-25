using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace OfflineWebsiteViewer
{
    public class CustomDownloadHandler : IDownloadHandler
    {
        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    if (downloadItem.Url.StartsWith("file://"))
                    {
                        try
                        {
                            var fileUri = new Uri(downloadItem.Url);
                            Process.Start(fileUri.AbsolutePath);
                        }
                        catch (Exception)
                        {
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
