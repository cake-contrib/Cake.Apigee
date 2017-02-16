using System.Collections;
using System.Collections.Generic;

namespace Cake.Apigee.Contracts
{    
    public class DeployProxyResult
    {
        public string State { get; set; }

        public IEnumerable<DeployEnvironment> Environment { get; set; }
    }
}