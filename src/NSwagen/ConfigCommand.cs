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
    [Description("Helps to create or update the configuration file for the specified package details.", Name = "config")]
    public class ConfigCommand : OaktonAsyncCommand<ConfigInput>
    {
        public ConfigCommand()
        {
            Usage("Configuration init").Arguments(x => x.Action);
        }

        public override async Task<bool> Execute(ConfigInput input)
        {
            try
            {
                if (input is null)
                    throw new ArgumentNullException(nameof(input));

                ConsoleWriter.Write(ConsoleColor.DarkCyan, $"Processing {input.Action} command request....");
                string outputPath = Helper.ResolveOutputPath(input.OutputFlag, "nswagen.config.json");
                List<GeneratorConfiguration> configurations;
                if (input.Action == ConfigInput.CommandAction.init)
                    configurations = await ProcessInitRequest(input).ConfigureAwait(false);
                else
                {
                    ConsoleWriter.Write(ConsoleColor.DarkCyan, $"Locating for {outputPath} file....");
                    if (!File.Exists(outputPath))
                    {
                        ConsoleWriter.Write(ConsoleColor.Red,
                            $"Config file {outputPath} does not exist to add the configuration.");
                        return false;
                    }

                    configurations = await ProcessAddRequest(input, outputPath).ConfigureAwait(false);
                }

                var result = ConfigurationSerializer(configurations);
                await File.WriteAllTextAsync(outputPath, result).ConfigureAwait(false);
                string action = input.Action == ConfigInput.CommandAction.init ? "generated" : "updated";
                ConsoleWriter.Write(ConsoleColor.DarkGreen, $"File {outputPath} is {action} successfully.");

                return true;
            }
            catch (Exception e)
            {
                ConsoleWriter.Write(ConsoleColor.Red, e.InnerException is null ? e.Message : e.InnerException.Message);
            }

            return default;
        }

        private static async Task<List<GeneratorConfiguration>> ProcessInitRequest(ConfigInput input)
        {
            return await BuildGeneratorConfigurations(input).ConfigureAwait(false);
        }

        private static async Task<List<GeneratorConfiguration>> ProcessAddRequest(ConfigInput input, string configPath)
        {
            List<GeneratorConfiguration> configurations = new ();

            //add the new configuration to existing config file
            var configModels = await ConfigurationHelper.ConfigurationDeserializer(configPath).ConfigureAwait(false);

            if (configModels is { Count: > 0 })
                configurations.AddRange(configModels);

            configurations.AddRange(await BuildGeneratorConfigurations(input).ConfigureAwait(false));

            return configurations;
        }

        private static string ConfigurationSerializer(List<GeneratorConfiguration> configurations)
        {
            var json = JsonSerializer.Serialize(configurations, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                IgnoreNullValues = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            return json;
        }

        private static async Task<List<GeneratorConfiguration>> BuildGeneratorConfigurations(ConfigInput input)
        {
            List<GeneratorConfiguration> configurations = new List<GeneratorConfiguration>();

            GeneratorConfiguration configuration = new ()
            {
                Swagger = string.IsNullOrEmpty(input.SwaggerFlag) ? "file|url" : input.SwaggerFlag,
                Output = string.IsNullOrEmpty(input.ClientOutputFlag) ? string.Empty : input.ClientOutputFlag,
            };

            if (string.IsNullOrEmpty(input.PackageFlag) && string.IsNullOrEmpty(input.SourceFlag) &&
                string.IsNullOrEmpty(input.AssemblyFlag))
            {
                configuration.Generator = new Generator()
                {
                    Package = "<package name>",
                    Source = "https://api.nuget.org/v3/index.json",
                    Assembly = "<assembly name>",
                    Name = "<generator name>",
                    Properties = new Dictionary<string, object>(),
                };
            }
            else
            {
                configuration.Generator = new Generator()
                {
                    Package = input.PackageFlag,
#pragma warning disable S3358 // Ternary operators should not be nested
                    Source = string.IsNullOrEmpty(input.PackageFlag)
                        ? null
                        : (string.IsNullOrEmpty(input.SourceFlag)
                            ? "https://api.nuget.org/v3/index.json"
                            : input.SourceFlag),
#pragma warning restore S3358 // Ternary operators should not be nested
                    Assembly = input.AssemblyFlag,
                    Discover = false,
                };
            }

            if (!string.IsNullOrEmpty(input.PackageFlag) || !string.IsNullOrEmpty(input.AssemblyFlag))
            {
                ClientGenerator generator = new (configuration);
                generator.OnStatus += (sender, args) => ConsoleWriter.Write(ConsoleColor.DarkCyan, $"{args.Message}");
                var generatorInfo = await generator.GetGeneratorInfo().ConfigureAwait(false);

                configuration.Generator.Name = string.IsNullOrEmpty(generatorInfo.Generator.Name)
                    ? "<generator name>"
                    : generatorInfo.Generator.Name;

                if (generatorInfo.Generator.GeneratorProperties != null && generatorInfo.Generator.GeneratorProperties.Any())
                {
                    configuration.Generator.Properties ??= new Dictionary<string, object>();

                    foreach (var generatorProperty in generatorInfo.Generator.GeneratorProperties!)
                    {
                        if (input.PropFlag.All(p => p.Key != generatorProperty.Name))
                        {
                            configuration.Generator.Properties.Add(generatorProperty.Name,
                                generatorProperty.DefaultValue);
                        }
                        else
                        {
                            var propMatch = input.PropFlag.First(p => p.Key == generatorProperty.Name);

                            object propValue = propMatch.Value;
                            if (generatorProperty.DataType!.Contains("bool", StringComparison.OrdinalIgnoreCase))
                                propValue = bool.TryParse(propMatch.Value, out bool _);
                            else if (generatorProperty.DataType!.Contains("int", StringComparison.OrdinalIgnoreCase))
                                propValue = int.TryParse(propMatch.Value, out int _);

                            configuration.Generator.Properties.Add(generatorProperty.Name, propValue);
                        }
                    }
                }
            }

            configurations.Add(configuration);
            return configurations;
        }
    }
}
