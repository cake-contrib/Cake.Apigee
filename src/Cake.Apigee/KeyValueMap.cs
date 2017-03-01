using System.Collections.Generic;

namespace Cake.Apigee
{
    public class KeyValueMap
    {
        public string Name { get; set; }

        public bool Encrypted { get; set; }

        public IEnumerable<KeyValueMapEntry> Entry { get; set; }
    }
}
