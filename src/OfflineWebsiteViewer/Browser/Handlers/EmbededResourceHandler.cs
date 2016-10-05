using System;
using System.IO;
using System.Net;
using System.Reflection;
using CefSharp;
using Cookie = CefSharp.Cookie;

namespace OfflineWebsiteViewer.Browser.Handlers
{
    class EmbededResourceHandler : IResourceHandler
    {
        private string _mimeType;
        private Stream _stream;
        private readonly Assembly _assembly;
        private readonly string _resourceName;

        public EmbededResourceHandler(Assembly assembly, string resourceName)
        {
            _assembly = assembly;
            _resourceName = resourceName;
        }

        bool IResourceHandler.ProcessRequest(IRequest request, ICallback callback)
        {
            _stream = _assembly.GetManifestResourceStream(_resourceName);
            if (_stream != null)
            {
                var fileExtension = Path.GetExtension(_resourceName);
                _mimeType = ResourceHandler.GetMimeType(fileExtension);
                callback.Continue();
                return true;
            }

            callback.Dispose();
            return false;
        }


        void IResourceHandler.GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            responseLength = _stream?.Length ?? 0;
            redirectUrl = null;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusText = "OK";
            response.MimeType = _mimeType;
        }

        bool IResourceHandler.ReadResponse(Stream dataOut, out int bytesRead, ICallback callback)
        {
            //Dispose the callback as it's an unmanaged resource, we don't need it in this case
            callback.Dispose();

            if (_stream == null)
            {
                bytesRead = 0;
                return false;
            }

            //Data out represents an underlying buffer (typically 32kb in size).
            var buffer = new byte[dataOut.Length];
            bytesRead = _stream.Read(buffer, 0, buffer.Length);

            dataOut.Write(buffer, 0, buffer.Length);

            return bytesRead > 0;
        }

        bool IResourceHandler.CanGetCookie(Cookie cookie)
        {
            return true;
        }

        bool IResourceHandler.CanSetCookie(Cookie cookie)
        {
            return true;
        }

        void IResourceHandler.Cancel()
        {

        }

        void IDisposable.Dispose()
        {

        }
    }
}
