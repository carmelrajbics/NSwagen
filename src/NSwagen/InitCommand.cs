using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NSwagen.Cli.Inputs;
using NSwagen.Core;
using NSwagen.Core.Models;
using Oakton;

namespace NSwagen.Cli
{
    [Description("Creates the configuration file with predefined values for a specified package.")]
    public class InitCommand : OaktonAsyncCommand<InitInput>
    {
        public override async Task<bool> Execute(InitInput input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            ConsoleWriter.Write(ConsoleColor.DarkCyan, "Processing Init command request....");

            var result = await ProcessInitRequest(input).ConfigureAwait(false);
            string outputPath = Helper.ResolveOutputPath(input.OutputFlag, "nswagen.config.json");
            await File.WriteAllTextAsync(outputPath, result).ConfigureAwait(false);
            ConsoleWriter.Write(ConsoleColor.DarkGreen, $"File {outputPath} is generated successfully.");

            return true;
        }

        private static async Task<string> ProcessInitRequest(InitInput input)
        {
            GeneratorConfiguration configuration = new ()
            {
                Swagger = "file|url",
                Generator = new ()
                {
                    Package = string.IsNullOrEmpty(input.PackageFlag) ? "<package name>" : input.PackageFlag,
                    Source = string.IsNullOrEmpty(input.SourceFlag) ? "https://api.nuget.org/v3/index.json" : input.SourceFlag,
                    Assembly = string.IsNullOrEmpty(input.AssemblyFlag) ? "<assembly file path>" : input.AssemblyFlag,
                    Discover = false,
                },
            };
            if (string.IsNullOrEmpty(input.PackageFlag) && string.IsNullOrEmpty(input.AssemblyFlag))
                configuration.Generator.Name = "<generator name>";
            else
            {
                ClientGenerator generator = new (configuration);
                generator.OnStatus += (sender, args) => ConsoleWriter.Write(ConsoleColor.DarkCyan, $"{args.Message}");
                var generatorInfo = await generator.GetGeneratorInfo().ConfigureAwait(false);

                configuration.Generator.Name = string.IsNullOrEmpty(generatorInfo.Generator.Name)
                    ? "<generator name>"
                    : generatorInfo.Generator.Name;

                configuration.Generator.Properties ??= new Dictionary<string, object>();

                configuration.Generator.Properties = generatorInfo.Generator.GeneratorProperties!.ToDictionary<GeneratorProperty, string, object>(
                    generatorProperty => generatorProperty.Name,
                    generatorProperty => generatorProperty.DefaultValue);
            }

            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            return json;
        }
    }
}
