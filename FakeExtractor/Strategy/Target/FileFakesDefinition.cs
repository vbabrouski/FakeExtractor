using System.Collections.Generic;
using System.Linq;

namespace FakeExtractor.Strategy.Target
{
    public class FileFakesDefinition
    {
        public string Path { get; set; }
        public ICollection<string> Namespaces { get; } = new List<string>();
        public ICollection<string> Shims { get; } = new List<string>();
        public ICollection<string> Stubs { get; } = new List<string>();

        public bool HasFakes => HasNamespaces || HasShims || HasStubs;
        public bool HasNamespaces => Namespaces?.Count > 0;
        public bool HasShims => Shims?.Count > 0;
        public bool HasStubs => Stubs?.Count > 0;

        public IEnumerable<string> AllTypeCombinations => Namespaces.SelectMany(n => Shims.Concat(Stubs).Select(s => $"{n}.{s}")).Concat(Shims).Concat(Stubs);
    }
}