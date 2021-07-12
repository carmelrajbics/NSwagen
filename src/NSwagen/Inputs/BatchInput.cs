using Oakton;

namespace NSwagen.Cli.Inputs
{
    public class BatchInput
    {
        [Description("The configuration file defining the route information.", Name = "config")]
        public string ConfigurationFile { get; set; } = null!;
    }
}
