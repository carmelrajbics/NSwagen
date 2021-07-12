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
    [Description("Generates client code for the specified package or assembly in batches.")]
    public class BatchCommand : OaktonAsyncCommand<BatchInput>
    {
        public override async Task<bool> Execute(BatchInput input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            ConsoleWriter.Write(ConsoleColor.DarkCyan, "Processing Batch command request....");
            return await ProcessBatchRequest(input).ConfigureAwait(false);
        }

        private static async Task<bool> ProcessBatchRequest(BatchInput input)
        {
            if (!File.Exists(input.ConfigurationFile))
                throw new Exception("Configuration file not found.");

            var configString = await File.ReadAllTextAsync(input.ConfigurationFile).ConfigureAwait(false);

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
