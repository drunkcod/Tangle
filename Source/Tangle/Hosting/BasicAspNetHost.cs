using System;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Tangle.Web;

namespace Tangle.Hosting
{
    public class BasicAspNetHost : MarshalByRefObject
    {
        readonly HttpListener listener = new HttpListener();

        public static BasicAspNetHost CreateHost(string sitePath, Uri rootPath) {
            var host = (BasicAspNetHost)ApplicationHost.CreateApplicationHost(typeof(BasicAspNetHost), rootPath.LocalPath, sitePath);
            host.AddPrefix(rootPath.ToString());
            return host;
        }

        public void Start() {
            listener.Start();
            listener.BeginGetContext(HandleRequest, listener);
        }

        public void Stop() {
            listener.Stop();
        }

        void AddPrefix(string prefix) {
            listener.Prefixes.Add(prefix);
        }

        static void HandleRequest(IAsyncResult async) {
            var listener = (HttpListener)async.AsyncState;
            listener.BeginGetContext(HandleRequest, listener);
            var context = listener.EndGetContext(async);
            HttpRuntime.ProcessRequest(new HttpListenerWorkerRequest(context));
        }
    }
}
