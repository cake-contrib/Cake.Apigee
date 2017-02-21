using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Cake.Apigee.Contracts;
using Cake.Apigee.Services;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Apigee
{
    [CakeAliasCategory("Apigee")]
    public static class ApigeeAliases
    {
        public static ApigeeProxyManagementService ApigeeProxyManagementService { get; set; } = new ApigeeProxyManagementService(new HttpClient());

        [CakeMethodAlias]
        public static ImportProxyResult ImportProxy(this ICakeContext ctx, string orgName, string proxyName, FilePath proxyZipfile, ImportProxySettings settings = null)
        {
            ctx.Log.Information("Import proxy " + proxyZipfile);

            return Run(() => ApigeeProxyManagementService.ImportProxyAsync(ctx, orgName, proxyName, proxyZipfile, settings).Result);
        }

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
