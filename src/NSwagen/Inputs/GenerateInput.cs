using System.Collections.Generic;
using Oakton;

namespace NSwagen.Cli.Inputs
{
    public class GenerateInput : BaseInput
    {
        [FlagAlias("swagger", true)]
        [Description("The swagger document file or url path used to generate client.")]
        public string SwaggerFlag { get; set; } = null!;

        [Description("The name of client generator.")]
        public string NameFlag { get; set; } = null!;

        [FlagAlias("discover", 'd')]
        [Description("Discover the generator details by the tool.")]
        public bool DiscoverFlag { get; set; }

        [FlagAlias("output", 'o')]
        [Description("Directory to create the client. If not specified, current directory is used.")]
        public string? OutputFlag { get; set; }

        [FlagAlias("config", 'c')]
        [Description("The configuration file defining the client generator information.")]
        public string? ConfigurationFileFlag { get; set; }

        [FlagAlias("prop")]
        [Description("The property required for client generation.")]
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable S1104 // Fields should not have public accessibility
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable SA1401 // Fields should be private
        public Dictionary<string, string> PropFlag = new Dictionary<string, string>();
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
