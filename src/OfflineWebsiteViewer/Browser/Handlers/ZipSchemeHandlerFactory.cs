using System.Windows;
using CefSharp;
using OfflineWebsiteViewer.Project;

namespace OfflineWebsiteViewer.Browser.Handlers
{
    class ZipSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public static string SchemeName = "zip";

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            var project = (Application.Current as App)?.Project as ArchiveProject;

            if(project?.Archive != null)
                return new ZipResourceHandler(project.Archive);
            return new ResourceHandler();
        }
    }
}
