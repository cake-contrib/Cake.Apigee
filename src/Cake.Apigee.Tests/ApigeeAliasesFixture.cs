using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using Cake.Apigee.Services;
using Cake.Core;

using Moq;

namespace Cake.Apigee.Tests
{
    public class ApigeeAliasesFixture : IDisposable
    {
        private string proxyZipFile;

        public ApigeeProxyManagementService ApigeeProxyManagementService { get; } 

        public Mock<ICakeContext> ContextMock { get; set; }

        public FakeResponseHandler FakeResponseHandler { get; set; } = new FakeResponseHandler();

        public Uri RequestUrl
        {
            get
            {
                if (this.FakeResponseHandler.Requests == null || !this.FakeResponseHandler.Requests.Any())
                {
                    return null;
                }

                var firstRequest = this.FakeResponseHandler.Requests.First();
                return firstRequest.RequestUri;
            }
        }

        public ApigeeAliasesFixture()
        {
            this.ApigeeProxyManagementService = new ApigeeProxyManagementService(new HttpClient(FakeResponseHandler));
            this.ContextMock = new Mock<ICakeContext>();
        }

        public string GetProxyZipFilePath()
        {
            proxyZipFile = Path.GetTempPath() + Guid.NewGuid() + ".zip";
            ResourceHelper.CopyResourceToFileAsync("weatherapi.zip", proxyZipFile).Wait();
            return proxyZipFile;
        }

        public void UseSuccessfulImportResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ResourceHelper.GetResourceAsString("ImportProxyResponse.json"))
                };

            this.FakeResponseHandler.SetFakeResponse(response);
        }

        public void Dispose()
        {
            this.FakeResponseHandler = new FakeResponseHandler();
            this.ContextMock = new Mock<ICakeContext>();
            if (proxyZipFile != null)
            {
                File.Delete(proxyZipFile);
            }
        }
    }
}
