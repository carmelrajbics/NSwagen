namespace NSwagen.Core.Models
{
    public class GeneratorConfiguration
    {
        public string Swagger { get; set; } = null!;

        public string? Output { get; set; } = null!;

        public Generator Generator { get; set; } = new ();
    }
}
