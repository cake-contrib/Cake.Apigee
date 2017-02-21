using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cake.Apigee.Contracts
{
    public class ApiProxyMetadata
    {
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }

        public string SubType { get; set; }
    }
}
