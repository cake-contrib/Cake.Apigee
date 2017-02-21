using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using Cake.Apigee.Services;
using Cake.Core;
using Cake.Core.Diagnostics;

using Moq;

using Xunit.Abstractions;

using Verbosity = Cake.Core.Diagnostics.Verbosity;

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

        public ApigeeAliasesFixture(ITestOutputHelper output)
        {
            this.ApigeeProxyManagementService = new ApigeeProxyManagementService(new HttpClient(FakeResponseHandler));
            this.ContextMock = new Mock<ICakeContext>();
            this.ContextMock.Setup(
                cm =>
                    cm.Log.Write(
                        It.IsAny<Verbosity>(),
                        It.IsAny<LogLevel>(),
                        It.IsAny<string>(),
                        It.IsAny<object[]>())).Callback<Verbosity, LogLevel, string, object[]>(
                            (v, l, m, p) =>
                            {
                                output.WriteLine($"{v}-{l}: {string.Format(m, p)}{Environment.NewLine}");
                            });
        }

        public string GetProxyZipFilePath()
        {
            proxyZipFile = Path.GetTempPath() + Guid.NewGuid() + ".zip";
            ResourceHelper.CopyResourceToFileAsync("weatherapi.zip", proxyZipFile).Wait();
            return proxyZipFile;
        }

        public void UseSuccessfulImportResponse()
        {
            SetFakeResponse(HttpStatusCode.OK, "ImportProxyResponse.json");
        }

        public void UseFailedImportResponse()
        {
            SetFakeResponse(HttpStatusCode.BadRequest, "ImportProxyFailResponse.json");
        }

        public void UseSuccessfulNpmInstallResponse()
        {            
            SetFakeResponse(HttpStatusCode.OK, "InstallNodePackagedModulesResponse.json");
        }

        public void UseSuccessfulGetApiProxyResponse()
        {
            SetFakeResponse(HttpStatusCode.OK, "GetApiProxyResponse.json");
        }

        public void Use(HttpStatusCode code, string resourceName)
        {
            SetFakeResponse(code, resourceName);
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

        private void SetFakeResponse(HttpStatusCode statusCode, string resourceName)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode)
            {
                Content =
                    new StringContent(
                        ResourceHelper.GetResourceAsString(resourceName))
            };

            this.FakeResponseHandler.SetFakeResponse(response);
        }       
    }
}
