using System;

namespace Cake.Apigee
{
    public class DeployProxySettings : BaseSettings
    {
        public bool? Override { get; set; } = true;

        public TimeSpan? Delay { get; set; } = TimeSpan.FromSeconds(15);
    }
}
