using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Cake.Apigee.Contracts;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Cake.Apigee.Services
{
    public class ApigeeProxyManagementService
    {
        #region Private Member Variables

        private readonly Uri baseUri = new Uri("https://api.enterprise.apigee.com");

        private readonly HttpClient client;

        private readonly JsonSerializerSettings ApigeeJsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        #endregion

        #region  Constructors

        public ApigeeProxyManagementService(HttpClient client)
        {
            this.client = client;

            // Commands such as NPM install sometimes need more than the default
            this.client.Timeout = TimeSpan.FromMinutes(3);
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
                         + $"v1/o/{orgName}/environments/{envName}/apis/{proxyName}/revisions/{revisionNumber}/deployments";
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
                    if (result.Environment is JArray)
                    {
                        var environments = ((JArray)result.Environment).ToObject<IEnumerable<DeployEnvironment>>();
                        environment =                            
                            environments.FirstOrDefault(
                                env =>
                                env.Revision == revisionNumber 
                                && env.State == "deployed"
                                && env.Environment == envName);
                    }
                    else
                    {
                        environment = ((JObject)result.Environment).ToObject<DeployEnvironment>();

                        // Dirty
                        if (environment.State != "deployed")
                        {
                            environment = null;
                        }
                    }
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
            string url = baseUri + $"v1/organizations/{orgName}/apis?action=import&name={proxyName}&validate={settings.Validate.ToString().ToLowerInvariant()}";
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

        public async Task<NodePackagedModuleMetadata[]> InstallNodePackagedModules(
            ICakeContext ctx,
            string orgName,
            string proxyName,
            string revisionNumber,
            InstallNodePackagedModulesSettings settings)
        {
            ctx.Log.Information("Installing Node Packaged Modules (npm) in Apigee for revision {0} of {1}", revisionNumber, proxyName);
            string url = baseUri + $"v1/organizations/{orgName}/apis/{proxyName}/revisions/{revisionNumber}/npm";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Content = new FormUrlEncodedContent(new Dictionary<string, string> {{ "command", "install" }});

                var modules = await SendMessage<NodePackagedModuleMetadata[]>(ctx, message, settings);
                ctx.Log.Information(string.Join(Environment.NewLine, modules.Select(m => $"{m.Name}: {m.Version}")));
                return modules;
            }
        }

        public async Task<ApiProxy> GetApiProxy(ICakeContext ctx, string orgName, string proxyName, ICredentialSettings settings)
        {
            // https://api.enterprise.apigee.com
            string url = baseUri + $"v1/organizations/{orgName}/apis/{proxyName}";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                
                return await SendMessage<ApiProxy>(ctx, message, settings);
            }
        }

        public async Task<ApiProxy> DeleteApiProxyRevision(ICakeContext ctx, string orgName, string proxyName, string revisionNumber, ICredentialSettings settings)
        {
            var url = baseUri + $"v1/organizations/{orgName}/apis/{proxyName}/revisions/{revisionNumber}";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await SendMessage<ApiProxy>(ctx, message, settings);
            }
        }

        public async Task DeleteAllUndeployedApiProxyRevisions(ICakeContext ctx, string orgName, string proxyName, ICredentialSettings settings)
        {
            var proxy = await GetApiProxy(ctx, orgName, proxyName, settings);
            var tasks = new List<Task>();
            foreach (var revision in proxy.Revision)
            {
                tasks.Add(TryDeleteApiProxyRevision(ctx, orgName, proxyName, revision, settings));
            }

            Task.WaitAll(tasks.ToArray());            
        }

        public async Task<CreateKeyValueMapResult> CreateKeyValueMap(ICakeContext ctx, string orgName, KeyValueMap keyValueMap, IEnvironmentSettings settings)
        {
            ctx.Log.Information("Creating a KeyValueMap in Apigee");
            var url = ConstructKeyValueMapUrl(orgName, settings);

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }                

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = JsonConvert.SerializeObject(keyValueMap, Formatting.None, ApigeeJsonSettings);
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return await SendMessage<CreateKeyValueMapResult>(ctx, message, settings);
            }
        }

        public async Task<DeleteKeyValueMapResult> DeleteKeyValueMap(ICakeContext ctx, string orgName, string keyValueMapName, IEnvironmentSettings settings)
        {
            ctx.Log.Information("Deleting KeyValueMap {0} in Apigee", keyValueMapName);
            var url = ConstructKeyValueMapUrl(orgName, settings);
            url += "/" + keyValueMapName;

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await SendMessage<DeleteKeyValueMapResult>(ctx, message, settings);
            }
        }

        public async Task<IEnumerable<string>> ListKeyValueMaps(ICakeContext ctx, string orgName, IEnvironmentSettings settings)
        {
            ctx.Log.Information("Listing KeyValueMaps in Apigee");
            var url = ConstructKeyValueMapUrl(orgName, settings);            

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await SendMessage<string[]>(ctx, message, settings);
            }
        }

        private string ConstructKeyValueMapUrl(string orgName, IEnvironmentSettings settings)
        {
            var url = baseUri + $"v1/organizations/{orgName}";
            if (settings != null && settings.Environment != null)
            {
                url += $"/environments/{settings.Environment}";
            }

            url += "/keyvaluemaps";
            return url;
        }

        private async Task<T> SendMessage<T>(ICakeContext ctx, HttpRequestMessage message, ICredentialSettings settings)
        {            
            using (HttpResponseMessage response = await client.SendAsync(message))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string error = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                    ctx.Log.Error("Apigee status {0} returned: {1}", response.StatusCode, error);
                    throw new Exception("Apigee returned " + response.StatusCode + ": " + message.Method + " " + message.RequestUri);
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

        private async Task<bool> TryDeleteApiProxyRevision(ICakeContext ctx, string orgName, string proxyName, string revisionNumber, ICredentialSettings settings)
        {
            var url = baseUri + $"v1/organizations/{orgName}/apis/{proxyName}/revisions/{revisionNumber}";
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                if (!string.IsNullOrEmpty(settings?.Credentials?.Username))
                {
                    AddAuthorization(settings.Credentials, message);
                }

                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (HttpResponseMessage response = await client.SendAsync(message))
                {
                    string body = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                    if (settings?.Debug ?? false)
                    {
                        Console.WriteLine();
                        Console.WriteLine("RESPONSE from " + message.RequestUri);
                        Console.WriteLine(body);
                    }

                    bool? result = null;
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ErrorResult>(body);
                        if (error.Code == "distribution.ApplicationCanNotBeDeleted")
                        {
                            result = false;
                        }                        
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        result = true;
                    }

                    ctx.Log.Verbose("Proxy revision {0} for {1} {2}", proxyName, revisionNumber, result ?? false ? "deleted" : "not deleted");
                    if (result == null)
                    {
                        throw new Exception($"Apigee returned unexpected status code '{response.StatusCode}' when deleting revision " + revisionNumber + " of proxy " + proxyName);
                    }

                    return result.Value;
                }
            }
        }

        #endregion        
    }
}