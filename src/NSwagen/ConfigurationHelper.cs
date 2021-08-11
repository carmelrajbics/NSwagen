using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NSwagen.Core.Models;

namespace NSwagen.Cli
{
    public static class ConfigurationHelper
    {
        internal static async Task<List<GeneratorConfiguration>?> ConfigurationDeserializer(string configurationFile)
        {
            var configString = await File.ReadAllTextAsync(configurationFile).ConfigureAwait(false);

            var configModels = JsonSerializer.Deserialize<List<GeneratorConfiguration>>(configString,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                });
            return configModels;
        }
    }
}
