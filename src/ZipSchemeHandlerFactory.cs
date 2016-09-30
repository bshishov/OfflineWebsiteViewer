using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using ICSharpCode.SharpZipLib.Zip;

namespace OfflineWebsiteViewer
{
    class ZipSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            var project = (Application.Current as App)?.Project;
            if (project != null)
                return new ZipResourceHandler(new ZipFile(project.ArchiveFilePath));
            return null;
        }
    }
}
