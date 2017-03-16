using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Cake.Apigee.Contracts
{    
    public class DeployProxyResult
    {
        public string State { get; set; }

        public JToken Environment { get; set; }
    }
}