using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.Apigee.Contracts
{
    public class ApiProxy
    {
        public string Name { get; set; }

        public IEnumerable<string> Revision { get; set; }

        public ApiProxyMetadata MetaData { get; set; }
    }
}
