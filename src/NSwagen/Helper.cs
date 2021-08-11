using System.IO;

namespace NSwagen.Cli
{
    internal static class Helper
    {
        internal static string ResolveOutputPath(string? output, string defaultFileName = "client.cs")
        {
            //validate output as a file or directory, if not specified default to current directory
            if (string.IsNullOrEmpty(output))
                return Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, defaultFileName);
            else
            {
                //Check if the path is file or directory
                if (Directory.Exists(output))
                    return Path.Combine(output, defaultFileName);
                else if (Path.HasExtension(output))
                {
                    var directoryName = Path.GetDirectoryName(output);
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName!);
                    return output;
                }
                else
                {
                    Directory.CreateDirectory(output);
                    return Path.Combine(output, defaultFileName);
                }
            }
        }
    }
}
