using System;
using System.Linq;
using System.Threading.Tasks;
using NSwagen.Cli.Inputs;
using NSwagen.Core;
using NSwagen.Core.Models;
using Oakton;

namespace NSwagen.Cli
{
    [Description("Lists the generator information.")]
    public class InfoCommand : OaktonAsyncCommand<BaseInput>
    {
        public override async Task<bool> Execute(BaseInput input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input?.PackageFlag) || !string.IsNullOrEmpty(input?.AssemblyFlag))
                {
                    ConsoleWriter.Write(ConsoleColor.DarkCyan, "Processing Info command request..");
                    return await ProcessInfoRequest(input).ConfigureAwait(false);
                }

                ConsoleWriter.Write(ConsoleColor.Red, "Provide either package or assembly details.");
                return true;
            }
            catch (Exception e)
            {
                ConsoleWriter.Write(ConsoleColor.Red, e.InnerException is null ? e.Message : e.InnerException.Message);
            }

            return default;
        }

        private static async Task<bool> ProcessInfoRequest(BaseInput input)
        {
            GeneratorConfiguration configuration = new ()
            {
                Generator = new Generator()
                {
                    Package = input!.PackageFlag,
                    Assembly = input.AssemblyFlag,
                    Source = input.SourceFlag,
                },
            };

            ClientGenerator generator = new (configuration);
            generator.OnStatus += (sender, args) => ConsoleWriter.Write(ConsoleColor.DarkCyan, $"{args.Message}");
            var generators = await generator.GetGeneratorInfo().ConfigureAwait(false);

            if (generators is not { Count: > 0 })
                ConsoleWriter.Write(ConsoleColor.DarkRed, "No Generators found.");
            else
            {
                foreach (var generatorInfo in generators)
                {
                    //Display the generator
                    ConsoleWriter.Write(ConsoleColor.Magenta, $"****Available Generators****");
                    ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                        $"Name : {generatorInfo.Generator.Name.ToString()}");
                    if (generatorInfo.Generator.Tags is { Length: > 0 })
                    {
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                            $"Tags : {string.Join(",", generatorInfo.Generator.Tags)}");
                    }

                    //Display the properties
                    ConsoleWriter.Write(Environment.NewLine);
                    ConsoleWriter.Write(ConsoleColor.Cyan, "Generator Properties");
                    if (generatorInfo.Generator.GeneratorProperties is null ||
                        !generatorInfo.Generator.GeneratorProperties.Any())
                    {
                        ConsoleWriter.Write(ConsoleColor.DarkCyan, "No generator properties available.");
                        return true;
                    }

                    foreach (var generatorProperty in generatorInfo.Generator.GeneratorProperties)
                    {
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1, $" Name : {generatorProperty.Name}");
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                            $" Description : {generatorProperty.Description}");
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                            $" Required : {generatorProperty.Required}");
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                            $" Default Value : {generatorProperty.DefaultValue}");
                        ConsoleWriter.WriteWithIndent(ConsoleColor.Cyan, 1,
                            $" DataType : {generatorProperty.DataType}");
                        ConsoleWriter.Write(Environment.NewLine);
                    }
                }
            }

            return true;
        }
    }
}
