using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NSwagen.Cli.Inputs;
using NSwagen.Core;
using NSwagen.Core.Models;
using Oakton;

namespace NSwagen.Cli
{
    [Description("Creates a single client code for the specified package or assembly. Provide either configuration file or package details.")]
    public class CreateCommand : OaktonAsyncCommand<CreateInput>
    {
        public override async Task<bool> Execute(CreateInput input)
        {
            try
            {
                if (input is null)
                    throw new ArgumentNullException(nameof(input), "Provide either configuration file or package details.");
                ConsoleWriter.Write(ConsoleColor.DarkCyan, "Processing Generate command request....");
                return await ProcessGenerateRequest(input).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ConsoleWriter.Write(ConsoleColor.Red, e.InnerException is null ? e.Message : e.InnerException.Message);
            }

            return default;
        }

        private static async Task<bool> ProcessGenerateRequest(CreateInput input)
        {
            var configuration = ParseConfiguration(input);
            if (!ValidateConfigModel(configuration))
                return false;

            ClientGenerator generator = new (configuration);
            generator.OnStatus += (sender, args) => ConsoleWriter.Write(ConsoleColor.DarkCyan, $"{args.Message}");
            await generator.Generate().ConfigureAwait(false);
            ConsoleWriter.Write(ConsoleColor.DarkGreen, $"File {configuration.Output} is generated successfully.");
            return true;
        }

        private static GeneratorConfiguration ParseConfiguration(CreateInput input)
        {
            var configuration = new GeneratorConfiguration();

            if (!string.IsNullOrEmpty(input.ConfigurationFileFlag))
            {
                if (!File.Exists(input.ConfigurationFileFlag))
                    throw new Exception("Configuration file not found.");

                configuration = JsonSerializer.Deserialize<GeneratorConfiguration>(
                    File.ReadAllText(input.ConfigurationFileFlag), new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                //overwrite the config values if any options specified in the command line
                if (configuration is null)
                    return configuration!;
                if (!string.IsNullOrEmpty(input.SwaggerFlag))
                    configuration.Swagger = input.SwaggerFlag;

                if (!string.IsNullOrEmpty(input.OutputFlag))
                    configuration.Output = input.OutputFlag;

                configuration.Output = Helper.ResolveOutputPath(configuration.Output);
                configuration.Generator ??= new ();
                if (!string.IsNullOrEmpty(input.PackageFlag))
                    configuration.Generator.Package = input.PackageFlag;

                if (!string.IsNullOrEmpty(input.SourceFlag))
                    configuration.Generator.Source = input.SourceFlag;

                if (!string.IsNullOrEmpty(input.NameFlag))
                    configuration.Generator.Name = input.NameFlag;

                if (!string.IsNullOrEmpty(input.AssemblyFlag))
                    configuration.Generator.Assembly = input.AssemblyFlag;

                if (input.DiscoverFlag)
                    configuration.Generator.Discover = input.DiscoverFlag;

                if (input.PropFlag is { Count: > 0 })
                {
                    configuration.Generator.Properties ??= new Dictionary<string, object>();
                    foreach (var (key, value) in input.PropFlag)
                    {
                        configuration.Generator.Properties.Add(key, value);
                    }
                }
            }
            else
            {
                configuration.Swagger = input.SwaggerFlag;
                configuration.Output = input.OutputFlag;
                configuration.Generator.Name = input.NameFlag;
                configuration.Generator.Package = input.PackageFlag;
                configuration.Generator.Assembly = input.AssemblyFlag;
                configuration.Generator.Discover = input.DiscoverFlag;
                configuration.Generator.Source = input.SourceFlag;

                if (input.PropFlag is { Count: > 0 })
                {
                    configuration.Generator.Properties ??= new Dictionary<string, object>();
                    foreach (var (key, value) in input.PropFlag)
                    {
                        configuration.Generator.Properties.Add(key, value);
                    }
                }
            }

            return configuration!;
        }

        private static bool ValidateConfigModel(GeneratorConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.Swagger))
            {
                ConsoleWriter.Write(ConsoleColor.Red, "Please specify valid swagger file or url path.");
                return false;
            }

            if (configuration.Generator is null)
                throw new Exception("Generator details cannot be empty");
            else
            {
                if (string.IsNullOrEmpty(configuration.Generator.Package) &&
                    string.IsNullOrEmpty(configuration.Generator.Assembly))
                {
                    ConsoleWriter.Write(ConsoleColor.Red, "Provide either package or assembly details.");
                    return false;
                }

                if (!string.IsNullOrEmpty(configuration.Generator.Name) || configuration.Generator.Discover)
                    return true;
                ConsoleWriter.Write(ConsoleColor.Red,
                    "Specify the name of the generator or enable discover option");
                return false;
            }
        }
    }
}
