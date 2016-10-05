using System;
using System.IO;
using System.Net;
using CefSharp;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Cookie = CefSharp.Cookie;

namespace OfflineWebsiteViewer.Browser.Handlers
{
    class ZipResourceHandler : IResourceHandler
    {
        private Stream _stream;
        private string _mimeType;

        private readonly ZipFile _zipFile;

        public ZipResourceHandler(ZipFile zipFile)
        {
            _zipFile = zipFile;
        }

        bool IResourceHandler.ProcessRequest(IRequest request, ICallback callback)
        {
            var uri = new Uri(request.Url);
            var fileName = uri.LocalPath;

            if (fileName.StartsWith("/"))
                fileName = fileName.Substring(1, fileName.Length - 1);

            var entry = _zipFile.GetEntry(fileName);
            if (entry != null)
            {
                using (var inputStream = _zipFile.GetInputStream(entry))
                {
                    var buffer = new byte[4096];
                    var memoryStream = new MemoryStream();
                    StreamUtils.Copy(inputStream, memoryStream, buffer);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    _stream?.Dispose();
                    _stream = memoryStream;
                }
                
                _mimeType = ResourceHandler.GetMimeType(Path.GetExtension(fileName));
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

        bool IResourceHandler.CanGetCookie(Cookie cookie) => true;

        bool IResourceHandler.CanSetCookie(Cookie cookie) => true;

        void IResourceHandler.Cancel()
        {
        }
        void IDisposable.Dispose()
        {
        }
    }
}
