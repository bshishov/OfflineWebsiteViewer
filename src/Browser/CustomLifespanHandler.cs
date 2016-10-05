using CefSharp;

namespace OfflineWebsiteViewer.Browser
{
    class CustomLifespanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
            IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            // Preserve new windows to be opened and load all popup urls in the same browser view
            // e.g. target="_blank" will open in current window
            browserControl.Load(targetUrl);
            newBrowser = null;
            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return true;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
        }
    }
}
