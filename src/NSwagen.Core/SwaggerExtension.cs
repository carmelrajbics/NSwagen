using System;
using System.Threading.Tasks;
using NSwag;

namespace NSwagen.Core
{
    public static class SwaggerExtension
    {
        internal static async Task<OpenApiDocument> ReadSwaggerDocumentAsync(this string input)
        {
            if (!input.IsJson() && !input.IsYaml())
            {
                if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (input.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                        input.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                        return await OpenApiYamlDocument.FromUrlAsync(input).ConfigureAwait(false);
                    else
                        return await OpenApiDocument.FromUrlAsync(input).ConfigureAwait(false);
                }
                else
                {
                    if (input.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                        input.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                        return await OpenApiYamlDocument.FromFileAsync(input).ConfigureAwait(false);
                    else
                        return await OpenApiDocument.FromFileAsync(input).ConfigureAwait(false);
                }
            }
            else
            {
                return input.IsYaml()
                    ? await OpenApiYamlDocument.FromYamlAsync(input).ConfigureAwait(false)
                    : await OpenApiDocument.FromJsonAsync(input).ConfigureAwait(false);
            }
        }

        private static bool IsJson(this string data)
        {
            return data.StartsWith("{", StringComparison.InvariantCulture);
        }

        private static bool IsYaml(this string data)
        {
            return !data.IsJson() && data.Contains("\n", StringComparison.InvariantCulture);
        }
    }
}
