using System;
using System.Reflection;
using CefSharp;

namespace OfflineWebsiteViewer.Browser.Handlers
{
    class ResourceSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public static string SchemeName = "resource";
        private readonly Assembly _assembly;
        private readonly string _prefix;

        public ResourceSchemeHandlerFactory(string nmspace)
        {
            _prefix = nmspace;
            _assembly = Assembly.GetExecutingAssembly();
        }

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            var uri = new Uri(request.Url);
            var fileName = uri.AbsolutePath;
            var resourceName = _prefix + fileName.Replace('/', '.');
            if(_assembly.GetManifestResourceInfo(resourceName) != null)
                return new EmbededResourceHandler(_assembly, resourceName);

            return new ResourceHandler();
        }
    }
}
