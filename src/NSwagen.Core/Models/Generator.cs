using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NSwagen.Core.Models
{
    public class Generator
    {
        public string? Package { get; set; }

        public string? Source { get; set; }

        public string? Assembly { get; set; }

        public string Name { get; set; } = null!;

        public bool Discover { get; set; }

        [JsonIgnore]
        public string[] Tags { get; set; } = null!;

#pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary<string, object> Properties { get; set; } = null!;
#pragma warning restore CA2227 // Collection properties should be read only

        [JsonIgnore]
        public IEnumerable<GeneratorProperty>? GeneratorProperties { get; set; }
    }
}
