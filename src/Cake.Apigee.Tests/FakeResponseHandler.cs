using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cake.Apigee.Tests
{
    public class FakeResponseHandler : DelegatingHandler
    {
        #region Private Member Variables

        private HttpResponseMessage singleResponse;

        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        /// <summary>
        /// The fake responses.
        /// </summary>
        private readonly Dictionary<Uri, HttpResponseMessage> fakeResponses =
            new Dictionary<Uri, HttpResponseMessage>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Add fake response when <paramref name="uri"/> is requested.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="responseMessage">
        /// The response message.
        /// </param>
        public void AddFakeResponse(Uri uri, HttpResponseMessage responseMessage)
        {
            this.fakeResponses.Add(uri, responseMessage);
        }

        /// <summary>
        /// Override all responses with a single response.
        /// </summary>
        /// <param name="responseMessage"></param>
        public void SetFakeResponse(HttpResponseMessage responseMessage)
        {
            this.singleResponse = responseMessage;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The send async.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            this.Requests.Add(request);

            if (this.singleResponse != null)
            {
                return Task.FromResult(this.singleResponse);
            }

            if (this.fakeResponses.ContainsKey(request.RequestUri))
            {
                return Task.FromResult(this.fakeResponses[request.RequestUri]);
            }
            else
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
            }
        }

        #endregion
    }
}
