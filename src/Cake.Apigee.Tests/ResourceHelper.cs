using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Cake.Apigee.Tests
{
    public class ResourceHelper
    {
        public static string GetResourceAsString(string name)
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ResourceHelper).Namespace + ".Resources." + name);
            if (resourceStream == null)
            {
                throw new Exception($"Cannot find embedded resource under Resources called {name}");
            }

            using (StreamReader reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }

        public static async Task CopyResourceToFileAsync(string resourceName, string file)
        {
            using (Stream output = File.Create(file))
            {
                using (
                    var stream =
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream(typeof(ResourceHelper).Namespace + ".Resources." + resourceName))
                {
                    await stream.CopyToAsync(output);
                }               
            }
        }
    }
}
