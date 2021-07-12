namespace NSwagen.Core.Models
{
    public class GeneratorProperty
    {
        public string Description { get; init; } = null!;

        public string DefaultValue { get; init; } = null!;

        public bool Required { get; init; }

        public string Name { get; init; } = null!;

        public string? DataType { get; init; }
    }
}
