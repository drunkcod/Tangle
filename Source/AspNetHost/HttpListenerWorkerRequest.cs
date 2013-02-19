using System;
using System.Net;
using System.Web;

namespace Tangle.AspNetHost
{
    public class HttpListenerWorkerRequest : HttpWorkerRequest
    {
        readonly HttpListenerContext context;

        public HttpListenerWorkerRequest(HttpListenerContext context)
        {
            this.context = context;
        }

        HttpListenerRequest Request { get { return context.Request; } }
        HttpListenerResponse Response { get { return context.Response; } }

        public override void EndOfRequest() { Response.Close(); }

        public override void FlushResponse(bool finalFlush) { Response.OutputStream.Flush(); }

        public override string GetHttpVerbName() { return Request.HttpMethod; }

        public override string GetHttpVersion() { return Request.ProtocolVersion.ToString(); }

        public override string GetLocalAddress() { return Request.Url.Host; }

        public override int GetLocalPort() { return Request.LocalEndPoint.Port; }

        public override string GetQueryString() { return Request.Url.Query; }

        public override string GetRawUrl() { return Request.RawUrl; }

        public override string GetRemoteAddress() { return Request.RemoteEndPoint.Address.ToString(); }

        public override int GetRemotePort() { return Request.RemoteEndPoint.Port; }

        public override string GetUriPath() { return Request.Url.AbsolutePath; }

        public override void SendKnownResponseHeader(int index, string value)
        {
            SendUnknownResponseHeader(HttpWorkerRequest.GetKnownResponseHeaderName(index), value);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            throw new NotImplementedException();
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            throw new NotImplementedException();
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            Response.OutputStream.Write(data, 0, length);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            Response.StatusCode = statusCode;
            Response.StatusDescription = statusDescription;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            Response.AddHeader(name, value);
        }

        public override bool IsEntireEntityBodyIsPreloaded() { return false; }

        public override byte[] GetPreloadedEntityBody() { return new byte[0]; }

        public override string GetKnownRequestHeader(int index) {
            return GetUnknownRequestHeader(HttpWorkerRequest.GetKnownRequestHeaderName(index));
        }

        public override string GetUnknownRequestHeader(string name) { return Request.Headers[name]; }

        public override int ReadEntityBody(byte[] buffer, int size) { return ReadEntityBody(buffer, 0, size); }

        public override int ReadEntityBody(byte[] buffer, int offset, int size) { return Request.InputStream.Read(buffer, offset, size); }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset) { throw new NotImplementedException(); }

        public override int GetPreloadedEntityBodyLength() { throw new NotImplementedException(); }

        public override long GetBytesRead() { throw new NotImplementedException(); }

        public override int GetTotalEntityBodyLength() { throw new NotImplementedException(); }
    }
}
