using Oakton;

namespace NSwagen.Cli.Inputs
{
    public class BaseInput
    {
        [Description("The client generator assembly.")]
        public string? AssemblyFlag { get; set; }

        [Description("The client generator package.")]
        public string? PackageFlag { get; set; }

        [Description("The package source.")]
        public string? SourceFlag { get; set; }
    }
}
