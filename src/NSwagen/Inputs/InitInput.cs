using Oakton;

namespace NSwagen.Cli.Inputs
{
    public class InitInput : BaseInput
    {
        [Description("Directory to create the client. If not specified, current directory is used.")]
        public string? OutputFlag { get; set; }
    }
}
