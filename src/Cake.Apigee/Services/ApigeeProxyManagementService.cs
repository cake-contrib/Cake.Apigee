using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Cake.Apigee.Contracts;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

using Newtonsoft.Json;

namespace Cake.Apigee.Services
{
    public class ApigeeProxyManagementService
    {
        #region Private Member Variables

        private readonly Uri baseUri = new Uri("https://api.enterprise.apigee.com");

        private readonly HttpClient client;

        #endregion

        #region  Constructors

        public ApigeeProxyManagementService(HttpClient client)
        {
            this.client = client;
        }

        #endregion

        #region Public Methods

        public async Task<DeployProxyResult> DeployProxyAsync(
            ICakeContext ctx,
            string orgName,
            string envName,
            string proxyName,
            string revisionNumber,
            DeployProxySettings settings)
        {
            ctx.Log.Information("Deploying {0} to Apigee environment {1}", proxyName, envName);
            string url = baseUri
                         + $"/v1/o/{orgName}/environments/{envName}/apis/{proxyName}/revisions/{revisionNumber}/deployments";
            List<string> queryParams = new List<string>();
            if (settings?.Override ?? false)
            {
                queryParams.Add("override=" + settings?.Override.Value);
            }

            if (settings?.Delay != null)
            {
                queryParams.Add("delay=" + settings?.Delay.Value.TotalSeconds);
            }

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams.ToArray());
            }

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");
                var result = await SendMessage<DeployProxyResult>(ctx, message, settings);

                // First deploy results in state being set
                if (result.State == "deployed")
                {
                    return result;
                }

                // Otherwise need to check the environments
                DeployEnvironment environment = null;                
                if (result.Environment != null)
                {

                    environment =
                        result.Environment.FirstOrDefault(
                            env =>
                                env.Revision == revisionNumber && env.State == "deployed"
                                && env.Environment == envName);
                }

                if (environment == null)
                {
                    throw new Exception("Did not find a successful deployment");
                }
            
                return result;
            }
        }

        public async Task<ImportProxyResult> ImportProxyAsync(
            ICakeContext ctx,
            string orgName,
            string proxyName,
            FilePath proxyZipfile,
            ImportProxySettings settings)
        {
            ctx.Log.Information("Importing Apigee proxy {0}", proxyName);
            string url = baseUri + $"/v1/organizations/{orgName}/apis?action=import&name={proxyName}";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    using (FileStream fileStream = File.OpenRead(proxyZipfile.ToString()))
                    {
                        content.Add(new StreamContent(fileStream), "file");
                        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        message.Content = content;
                        return await SendMessage<ImportProxyResult>(ctx, message, settings);
                    }
                }
            }
        }

        public async Task<InstallNodePackagedModulesResult[]> InstallNodePackagedModules(
            ICakeContext ctx,
            string orgName,
            string proxyName,
            string revisionNumber,
            InstallNodePackagedModulesSettings settings)
        {
            ctx.Log.Information("Installing Node Packaged Modules (npm) in Apigee for revision {0} of {1}", revisionNumber, proxyName);
            string url = baseUri + $"/v1/organizations/{orgName}/apis/{proxyName}/revisions/{revisionNumber}/npm";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "command", "install" }});
                return await SendMessage<InstallNodePackagedModulesResult[]>(ctx, message, settings);
            }
        }

        private async Task<T> SendMessage<T>(ICakeContext ctx, HttpRequestMessage message, BaseSettings settings)
        {
            using (HttpResponseMessage response = await client.SendAsync(message))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    ctx.Log.Error("Apigee status {0} returned: {1}", response.StatusCode, error);
                    throw new Exception("Apigee returned " + response.StatusCode);
                }

                string body = await response.Content.ReadAsStringAsync();
                if (settings?.Debug ?? false)
                {
                    Console.WriteLine();
                    Console.WriteLine("RESPONSE from " + message.RequestUri);
                    Console.WriteLine(body);
                }

                return JsonConvert.DeserializeObject<T>(body);
            }
        }

        #endregion

        #region Private Methods

        private static void AddAuthorization(Credentials credentials, HttpRequestMessage message)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue(
                                                "Basic",
                                                Convert.ToBase64String(
                                                    Encoding.ASCII.GetBytes(
                                                        $"{credentials.Username}:{credentials.Password}")));
        }

        #endregion
    }
}