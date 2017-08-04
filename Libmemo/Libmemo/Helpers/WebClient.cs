using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Libmemo {
    public class WebClient {

        private HttpClientHandler handler;
        private HttpClient client;

        private WebClient() {
            handler = new HttpClientHandler() { CookieContainer = Settings.Cookies != null ? Settings.Cookies : new CookieContainer() };
            client = new HttpClient(handler);
        }

        private static WebClient _instance = null;
        public static WebClient Instance {
            get {
                if (_instance == null) {
                    _instance = new WebClient();
                }

                return _instance;
            }
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, Uri uri, HttpContent content, int timeout, CancellationToken cancelToken) {
            var request = new HttpRequestMessage(method, uri);
            if (content != null) {
                request.Content = content;
            }

            var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            var groupTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancelToken);

            try {
                var res = await client.SendAsync(request, groupTokenSource.Token);
                Settings.Cookies = handler.CookieContainer;
                return res;
            } catch (OperationCanceledException) {
                if (timeoutTokenSource.Token.IsCancellationRequested) throw new TimeoutException();
                else throw;
            }
        }




    }

    public class TimeoutException : Exception {

    }
}
