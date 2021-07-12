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
        public static Task<int> Main(string[] args)
        {
            var executor = CommandExecutor.For(_ =>
            {
                // Find and apply all command classes discovered
                // in this assembly
                _.RegisterCommands(typeof(Program).GetTypeInfo().Assembly);
                _.SetAppName("NSwagen");
            });

#if DEBUG_TOOL
            var arguments = args.FirstOrDefault() ?? Console.ReadLine();

            while (!string.IsNullOrEmpty(arguments))
            {
                executor.ExecuteAsync(arguments);
                arguments = Console.ReadLine();
            }

            return Task.FromResult(0);
#else
            return Task.FromResult(executor.Execute(args));
#endif
        }
    }
}
