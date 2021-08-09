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
    [Description("Generates a multiple client code for the configuration specified in the config file.")]
    public class GenerateCommand : OaktonAsyncCommand<GenerateInput>
    {
        public override async Task<bool> Execute(GenerateInput input)
        {
            try
            {
                if (input is null)
                    throw new ArgumentNullException(nameof(input));
                ConsoleWriter.Write(ConsoleColor.DarkCyan, "Processing Batch command request....");
                return await ProcessBatchRequest(input).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ConsoleWriter.Write(ConsoleColor.Red, e.InnerException is null ? e.Message : e.InnerException.Message);
            }

            return default;
        }

        private static async Task<bool> ProcessBatchRequest(GenerateInput input)
        {
            if (!File.Exists(input.ConfigurationFileFlag))
            {
                //Check for the file in current directory
                string defaultConfigFile = "nswagen.config.json";
                ConsoleWriter.Write(ConsoleColor.DarkCyan,
                    $"Configuration file not specified. Attempting to locate {defaultConfigFile} file in current directory.");
                input.ConfigurationFileFlag = Path.Combine(Directory.GetCurrentDirectory(), defaultConfigFile);
                if (!File.Exists(input.ConfigurationFileFlag))
                    throw new Exception("Configuration file not found.");
            }

            var configString = await File.ReadAllTextAsync(input.ConfigurationFileFlag).ConfigureAwait(false);

            var configModels = JsonSerializer.Deserialize<List<GeneratorConfiguration>>(configString,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });

            if (configModels is { Count: <= 0 })
                throw new Exception("Error in mapping config file.");

            foreach (var configuration in configModels!)
            {
                configuration.Output = Helper.ResolveOutputPath(configuration.Output);
                ClientGenerator generator = new (configuration);
                generator.OnStatus += (sender, args) => ConsoleWriter.Write(ConsoleColor.DarkCyan, $"{args.Message}");
                await generator.Generate().ConfigureAwait(false);
                ConsoleWriter.Write(ConsoleColor.DarkGreen, $"File {configuration.Output} is generated successfully.");
            }

            return true!;
        }
    }
}
