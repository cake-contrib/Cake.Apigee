using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Cake.Apigee.Contracts;
using Cake.Apigee.Services;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Apigee
{
    /// <summary>
    /// Contains functionality for proxy import and deployments within Apigee.
    /// </summary>
    [CakeAliasCategory("Apigee")]
    public static class ApigeeAliases
    {
        public static ApigeeProxyManagementService ApigeeProxyManagementService { get; set; } = new ApigeeProxyManagementService(new HttpClient());

        /// <summary>
        /// Import a proxy into Apigee from a zip file. The zip file should contain an apiproxy folder at the top level.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// var result = ImportProxy(
        ///                orgName,
        ///                "myapi",
        ///                File("apiproxy.zip"),
        ///                new ImportProxySettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>
        /// This calls the following API:
        /// https://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/apis-0
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <param name="proxyZipfile">Path to the zip.</param>
        /// <param name="settings">The settings such as authentication credentials.</param>
        /// <returns>The result of the import such as the revision created.</returns>
        [CakeMethodAlias]
        public static ImportProxyResult ImportProxy(this ICakeContext ctx, string orgName, string proxyName, FilePath proxyZipfile, ImportProxySettings settings = null)
        {
            ctx.Log.Information("Import proxy " + proxyZipfile);

            return Run(() => ApigeeProxyManagementService.ImportProxyAsync(ctx, orgName, proxyName, proxyZipfile, settings ?? new ImportProxySettings()).Result);
        }

        /// <summary>
        /// Deploy a proxy within Apigee to an environment in Apigee using the results of an import. By default, if settings
        /// are supplied, they will use override and a 15 second delay to minimize downtime as described in:
        /// http://docs.apigee.com/api-services/content/deploy-api-proxies-using-management-api#seamless
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// DeployProxy(
        ///     orgName,
        ///     "dev",
        ///     result,
        ///     new DeployProxySettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>
        /// This calls the following API:
        /// https://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/environments/%7Benv_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_number%7D/deployments
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="envName">The name of the environment as shown in Apigee.</param>
        /// <param name="importResult">Using the result of the import saves re-specifying the name and revision.</param>
        /// <param name="settings">The setting such as authentication credentials.</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static DeployProxyResult DeployProxy(this ICakeContext ctx, string orgName, string envName, ImportProxyResult importResult, DeployProxySettings settings = null)
        {
            ctx.Log.Information("Deploy api " + importResult.Name);

            return DeployProxy(
                ctx,
                orgName,
                envName,
                importResult.Name,
                importResult.Revision,
                settings);
        }

        /// <summary>
        /// Deploy a proxy to Apigee using a specific revision number.
        /// </summary>
        /// <example>
        /// <para>Cake task to deploy revision 123 of myapi to the dev environment for myorg:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// DeployProxy(
        ///     orgName,
        ///     "dev",
        ///     "myapi",
        ///     "123",
        ///     new DeployProxySettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>
        /// This calls the following API:
        /// https://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/environments/%7Benv_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_number%7D/deployments
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="envName">The name of the environment as shown in Apigee.</param>
        /// <param name="apiName">Name of the proxy/api.</param>
        /// <param name="revisionNumber">The revision number of the proxy.</param>
        /// <param name="settings">The deployment settings such as the credentials. At least pass new settings to get the seamless deployment parameters described above.</param>
        /// <returns>Result of the deployment.</returns>
        [CakeMethodAlias]
        public static DeployProxyResult DeployProxy(
            this ICakeContext ctx,
            string orgName,
            string envName,
            string apiName,
            string revisionNumber,
            DeployProxySettings settings = null)
        {
            return Run(() => ApigeeProxyManagementService.DeployProxyAsync(ctx, orgName, envName, apiName, revisionNumber, settings).Result);
        }

        /// <summary>
        /// Run "npm install" on NodeJS embedded as a resource in the proxy. This can be useful if your proxy exceeds the 15MB limit imposed by Apigee.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// InstallNodePackagedModules(
        ///                    orgName,
        ///                    "myapi",
        ///                    "123",
        ///                    new InstallNodePackagedModulesSettings { Credentials = apigeeCredentials
        /// });        
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>
        /// This runs the following API with the command "install":
        /// https://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_num%7D/npm-0
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <param name="revisionNumber">The revision to deploy.</param>
        /// <param name="settings">The settings such as the authentication credentials.</param>
        /// <returns>The list of installed modules.</returns>
        [CakeMethodAlias]
        public static NodePackagedModuleMetadata[] InstallNodePackagedModules(
            this ICakeContext ctx,
            string orgName,
            string proxyName,
            string revisionNumber,
            InstallNodePackagedModulesSettings settings = null)
        {
            return ApigeeProxyManagementService.InstallNodePackagedModules(
                ctx,
                orgName,
                proxyName,
                revisionNumber,
                settings).Result;
        }

        /// <summary>
        /// Run "npm install" on NodeJS embedded as a resource in the proxy based on a previous import. This can be useful if your proxy exceeds the 15MB limit imposed by Apigee.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// InstallNodePackagedModules(
        ///                    orgName,
        ///                    importResult,
        ///                    new InstallNodePackagedModulesSettings { Credentials = apigeeCredentials
        /// });        
        /// ]]>
        /// </code>
        /// </example>        
        /// <remarks>
        /// This runs the following API with the command "install":
        /// https://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_num%7D/npm-0
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="importResult">The result of an import.</param>
        /// <param name="settings">Settings such as the authentication credentials.</param>
        /// <returns>The list of modules and versions that were installed.</returns>
        [CakeMethodAlias]
        public static NodePackagedModuleMetadata[] InstallNodePackagedModules(
            this ICakeContext ctx,
            string orgName,
            ImportProxyResult importResult,
            InstallNodePackagedModulesSettings settings = null)
        {
            return Run(() => ApigeeProxyManagementService.InstallNodePackagedModules(
                ctx,
                orgName,
                importResult.Name,
                importResult.Revision,
                settings).Result);
        }

        /// <summary>
        /// Retrieve API proxy metadata such as the list of revisions and when the proxy was created.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// var proxyMetadata = GetApiProxy(
        ///                    orgName,
        ///                    "myapi",
        ///                    new GetApiProxySettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>        
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="proxyName">Name of the proxy.</param>
        /// <param name="settings">The settings such as authentication credentials.</param>
        /// <returns>The proxy metadata.</returns>
        [CakeMethodAlias]
        public static ApiProxy GetApiProxy(
           this ICakeContext ctx,
           string orgName,
           string proxyName,
           GetApiProxySettings settings = null)
        {
            return Run(() => ApigeeProxyManagementService.GetApiProxy(
                ctx,
                orgName,
                proxyName,                
                settings).Result);
        }

        /// <summary>
        /// Delete a particular revision of a proxy.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// DeleteApiProxyRevision(
        ///                    orgName,
        ///                    "myapi",
        ///                    "123",
        ///                    new DeleteApiProxyRevisionSettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>        
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="proxyName">The name of the proxy.</param>
        /// <param name="revisionNumber">The revision to delete.</param>
        /// <param name="settings">Settings such as the authentication credentials.</param>
        [CakeMethodAlias]
        public static void DeleteApiProxyRevision(
           this ICakeContext ctx,
           string orgName,
           string proxyName,
           string revisionNumber,
           DeleteApiProxyRevisionSettings settings = null)
        {
            Run(() => ApigeeProxyManagementService.DeleteApiProxyRevision(
                ctx,
                orgName,
                proxyName,
                revisionNumber,
                settings).Result);
        }

        /// <summary>
        /// Delete all undeployed proxy revisions. This attempts to delete ALL proxy revisions but Apigee won't let
        /// already deployed revisions be removed.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// DeleteAllUndeployedApiProxyRevisions(
        ///                    orgName,
        ///                    "myapi",
        ///                    new DeleteAllUndeployedApiProxyRevisionsSettings { Credentials = apigeeCredentials });
        /// ]]>
        /// </code>
        /// </example>        
        /// <remarks>
        /// This makes calls to the following API for every revision of the proxy:
        /// https://docs.apigee.com/management/apis/delete/organizations/%7Borg_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_number%7D
        /// </remarks>
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="proxyName">The name of the proxy.</param>
        /// <param name="settings">The settings such as the authentication credentials.</param>
        [CakeMethodAlias]
        public static void DeleteAllUndeployedApiProxyRevisions(
           this ICakeContext ctx,
           string orgName,
           string proxyName,
           DeleteAllUndeployedApiProxyRevisionsSettings settings = null)
        {
            Run(() => ApigeeProxyManagementService.DeleteAllUndeployedApiProxyRevisions(
                ctx,
                orgName,
                proxyName,                
                settings).Wait());
        }

        /// <summary>
        /// Create an organisation or environment scoped key value map (KVM) in Apigee. 
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// CreateKeyValueMap(
        ///                 orgName,
        ///                 "myapi",
        ///                 new KeyValueMap
        ///                 {
        ///                     Name = "myMap",
        ///                     Encrypted = false,
        ///                     Entry = new[]
        ///                     {
        ///                         new KeyValueMapEntry { Name = "myKey", Value = "myValue" }
        ///                     }
        ///                  },
        ///                  new CreateKeyValueMapSettings { Credentials = apigeeCredentials, Environment = "dev" });
        /// ]]>
        /// </code>
        /// </example>        
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="keyValueMap">The values to set in the key value map.</param>
        /// <param name="settings">Settings such as credentials and an environment name if environment scoped.</param>
        [CakeMethodAlias]
        public static void CreateKeyValueMap(
           this ICakeContext ctx,
           string orgName,
           KeyValueMap keyValueMap,
           CreateKeyValueMapSettings settings = null)
        {
            Run(() => ApigeeProxyManagementService.CreateKeyValueMap(
                ctx,
                orgName,
                keyValueMap,
                settings).Wait());
        }

        /// <summary>
        /// Delete a key value map (KVM).
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// DeleteKeyValueMap(
        ///                    orgName,
        ///                    "myMap",
        ///                    new DeleteKeyValueMapSettings { Credentials = apigeeCredentials, Environment = "dev" });
        /// ]]>
        /// </code>
        /// </example>        
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="keyValueMapName">The name of the Key Value Map to delete.</param>
        /// <param name="settings">The settings such as authentication credentials and environment for environment scoped maps.</param>
        [CakeMethodAlias]
        public static void DeleteKeyValueMap(
           this ICakeContext ctx,
           string orgName,
           string keyValueMapName,
           DeleteKeyValueMapSettings settings = null)
        {
            Run(() => ApigeeProxyManagementService.DeleteKeyValueMap(
                ctx,
                orgName,
                keyValueMapName,
                settings).Wait());
        }

        /// <summary>
        /// List all of the key value maps at either organisation or environment scope.
        /// </summary>
        /// <example>
        /// <para>Cake task:</para>
        /// <code>
        /// <![CDATA[
        /// var orgName = "myorg";
        /// var apigeeCredentials = new Credentials
        /// {
        ///    Username = "me@me.com",
        ///    Password = "mypassword"
        /// };
        ///
        /// IEnumerable<string> list = ListKeyValueMaps(
        ///                    orgName,
        ///                    new ListKeyValueMapsSettings { Credentials = apigeeCredentials, Environment = "dev" });
        /// ]]>
        /// </code>
        /// </example>        
        /// <param name="ctx">The Cake context.</param>
        /// <param name="orgName">Your org name as shown in Apigee.</param>
        /// <param name="settings">Settings such as the Apigee authentication credentials.</param>
        /// <returns>The list of key value map names.</returns>
        [CakeMethodAlias]
        public static IEnumerable<string> ListKeyValueMaps(
           this ICakeContext ctx,
           string orgName,           
           ListKeyValueMapsSettings settings = null)
        {
            return Run(() => ApigeeProxyManagementService.ListKeyValueMaps(
                ctx,
                orgName,                
                settings).Result);
        }

        private static void Run(Action function)
        {
            try
            {
                function.Invoke();
            }
            catch (AggregateException aggEx)
            {
                throw new Exception(string.Join("; ", aggEx.Flatten().InnerExceptions.Select(ex => ex.Message)));
            }
        }

        private static T Run<T>(Func<T> function)
        {
            try
            {
                return function.Invoke();
            }
            catch (AggregateException aggEx)
            {                
                throw new Exception(string.Join("; ", aggEx.Flatten().InnerExceptions.Select(ex => ex.Message)));
            }
        }
    }
}
