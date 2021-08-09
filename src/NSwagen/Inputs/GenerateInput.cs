using Oakton;

namespace NSwagen.Cli.Inputs
{
    public class GenerateInput
    {
        [Description("The configuration file defining the multiple client generator information.", Name = "config")]
        public string ConfigurationFileFlag { get; set; } = null!;
    }
}
