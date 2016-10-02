using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using OfflineWebsiteViewer.Properties;
using Cookie = CefSharp.Cookie;

namespace OfflineWebsiteViewer.Handlers
{
    class EmbededResourceHandler : IResourceHandler
    {
        private string _prefix = "OfflineWebsiteViewer.Resources.Web";
        private string _mimeType;
        private Stream _stream;

        static EmbededResourceHandler()
        {
        }

        bool IResourceHandler.ProcessRequest(IRequest request, ICallback callback)
        {
            // The 'host' portion is entirely ignored by this scheme handler.
            var uri = new Uri(request.Url);
            var fileName = uri.AbsolutePath;
            fileName = fileName.Replace('/', '.');

            //var uri = new Uri(request.Url);
            //var fileName = uri.AbsolutePath;


            var assembly = Assembly.GetExecutingAssembly();

            _stream = assembly.GetManifestResourceStream(_prefix + fileName);
            if (_stream != null)
            {
                var fileExtension = Path.GetExtension(fileName);
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
