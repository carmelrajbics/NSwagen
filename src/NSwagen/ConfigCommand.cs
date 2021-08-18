using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleFx.Prompter;
using Microsoft.CSharp.RuntimeBinder;
using NSwagen.Cli.Inputs;
using NSwagen.Core;
using NSwagen.Core.Models;
using Oakton;

using CFxPrompter = ConsoleFx.Prompter.Prompter;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

                ConfigurationPrompter(input);

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

        private static void ConfigurationPrompter(ConfigInput input)
        {
            CFxPrompter.Style = Styling.Terminal;
            var prompter = new CFxPrompter();

            prompter.Confirm("DefaultConfiguration", "Do you want create a configuration with default values? ", true);

            prompter.Input(nameof(input.SwaggerFlag), $"Please provide the swagger file|url path: ", q => q
                .When(ans => string.IsNullOrEmpty(input.SwaggerFlag) && !ans.DefaultConfiguration)
                .WithInstructions("Format should be valid file|url path.")
                .ValidateInputWith((swagger, _) =>
                    swagger.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    swagger.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    swagger.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                    swagger.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
                    swagger.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .DefaultsTo("file|url"));

            prompter.Input(nameof(input.OutputFlag), $"Enter the directory path to create the config file: ", q => q
                .When(ans => string.IsNullOrEmpty(input.OutputFlag) && !ans.DefaultConfiguration)
                .DefaultsTo(string.Empty));

            prompter.Input(nameof(input.ClientOutputFlag), $"Enter the directory path to create the client proxies: ", q =>
                q
                    .When(ans => string.IsNullOrEmpty(input.ClientOutputFlag) && !ans.DefaultConfiguration)
                    .DefaultsTo(string.Empty));

            prompter.Input(nameof(input.PackageFlag), $"Enter the generator package name: ", q => q
                .When(ans => string.IsNullOrEmpty(input.PackageFlag) && !ans.DefaultConfiguration)
                .DefaultsTo(string.Empty));

            prompter.Input(nameof(input.SourceFlag), $"Enter the package source: ", q => q
                .When(ans => string.IsNullOrEmpty(input.SourceFlag) && !ans.DefaultConfiguration)
                .DefaultsTo(string.Empty));

            prompter.Input("Properties", $"Enter additional properties: ", q => q
                .WithInstructions("Allows only key:value pair with comma separated for multiple properties.")
                .ValidateInputWith((properties, _) => new Regex(@"\s*(.*?)\s*:\s*(.*?)\s*(,|$)").IsMatch(properties))
                .DefaultsTo(string.Empty)
                .When(ans => !ans.DefaultConfiguration));

            prompter.BetweenPrompts += (sender, args) => ConsoleWriter.Line();
            dynamic answers = prompter.Ask();

            if (answers.DefaultConfiguration)
                return;

            if (GetDynamicMember(answers, "SwaggerFlag"))
                input.SwaggerFlag = answers.SwaggerFlag;
            if (GetDynamicMember(answers, "OutputFlag"))
                input.OutputFlag = answers.OutputFlag;
            if (GetDynamicMember(answers, "ClientOutputFlag"))
                input.ClientOutputFlag = answers.ClientOutputFlag;
            if (GetDynamicMember(answers, "PackageFlag"))
                input.PackageFlag = answers.PackageFlag;
            if (GetDynamicMember(answers, "SourceFlag"))
                input.SourceFlag = answers.SourceFlag;

            if (!GetDynamicMember(answers, "Properties") || answers.Properties == null)
                return;

            string[] items = answers.Properties.TrimEnd(',').Split(',');
            foreach (string item in items)
            {
                string[] keyValue = item.Split(':');
                if (input.PropFlag.ContainsKey(keyValue[0]))
                    input.PropFlag[keyValue[0]] = keyValue[1];
                else
                    input.PropFlag.Add(keyValue[0], keyValue[1]);
            }
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
                        if (input.PropFlag.All(p => p.Key.Trim() != generatorProperty.Name.Trim()))
                        {
                            configuration.Generator.Properties.Add(generatorProperty.Name,
                                generatorProperty.DefaultValue);
                        }
                        else
                        {
                            var propMatch = input.PropFlag.First(p => p.Key.Trim() == generatorProperty.Name.Trim());

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

        private static bool GetDynamicMember(dynamic answers, string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, answers.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            return answers.TryGetMember(binder, out object result);
        }
    }
}
