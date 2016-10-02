using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace OfflineWebsiteViewer.Handlers
{
    class ResourceSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public static string SchemeName = "resource";

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            return new EmbededResourceHandler();
        }
    }
}
