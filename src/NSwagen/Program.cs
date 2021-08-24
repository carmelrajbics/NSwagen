using System;
#if DEBUG_TOOL
using System.Linq;
#endif
using System.Reflection;
using System.Threading.Tasks;
using Oakton;

namespace NSwagen.Cli
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var executor = CommandExecutor.For(_ =>
            {
                // Find and apply all command classes discovered
                // in this assembly
                _.RegisterCommands(typeof(Program).GetTypeInfo().Assembly);
                _.SetAppName("NSwagen");
            });

#if DEBUG_TOOL
            var promptArgs = Environment.GetEnvironmentVariable("promptArgs");
            if (string.Equals(promptArgs, "true", StringComparison.OrdinalIgnoreCase))
            {
                var arguments = args.FirstOrDefault() ?? Console.ReadLine();

                while (!string.IsNullOrEmpty(arguments))
                {
                    await executor.ExecuteAsync(arguments).ConfigureAwait(false);
                    arguments = Console.ReadLine();
                }

                return 0;
            }
            else
            {
                return await executor.ExecuteAsync(args).ConfigureAwait(false);
            }
#else
            return await executor.ExecuteAsync(args).ConfigureAwait(false);
#endif
        }
    }
}
