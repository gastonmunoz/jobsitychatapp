using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using System.Net;
using System.Text;

namespace JobsityChatBotHelperTests
{
    public class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
        public DelegatingHandlerStub()
        {
            _handlerFunc = (request, cancellationToken) => {
                byte[] bytes = Encoding.ASCII.GetBytes(@"Symbol,Date,Time,Open,High,Low,Close,Volume
AAPL.US,2022-09-29,22:00:07,146.1,146.72,140.68,142.48,127772886");
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK);
                ByteArrayContent content = new(bytes);
                response.Content = content;
                return Task.FromResult(response);
            };
        }

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}
