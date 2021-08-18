using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jeevan.NuGetClient;

namespace NSwagen.Core
{
    internal static class AssemblyResolverExtension
    {
        internal static async Task<(Assembly, string?, string?)> ExtractNugetPackage(this string package, string? source)
        {
            var packagePath = await DownloadLatestPackage(package,
                string.IsNullOrEmpty(source) ? "https://api.nuget.org/v3/index.json" : source).ConfigureAwait(false);

            //Extract nuget package
            var packageExtractPath = Path.ChangeExtension(packagePath.FullName, null);
            string packageZipPath = $"{packagePath.FullName}.zip";
            if (File.Exists(packageZipPath))
                File.Delete(packageZipPath);
            Directory.Move(packagePath.FullName, packageZipPath);

            if (Directory.Exists(packageExtractPath))
                Directory.Delete(packageExtractPath, true);
            ZipFile.ExtractToDirectory(packageZipPath, packageExtractPath);

            //Load assemblies
            var initialAssembly = await LoadAssemblies(packageExtractPath, package).ConfigureAwait(false);

            return (initialAssembly, packageZipPath, packageExtractPath);
        }

        private static async Task<FileInfo> DownloadLatestPackage(string package, string source)
        {
            try
            {
                //Read, download and extract nuget package from the source specified.
                using NuGetClient client = new (source);

                //Download nuget package
                FileInfo? packagePath = await client.DownloadLatestPackageAsync(package,
                    Path.GetTempPath(),
                    overwrite: true).ConfigureAwait(false);

                if (packagePath is null)
                    throw new Exception($"Could not find the package {package} in the source {source}");
                return packagePath;
            }
            catch (Exception)
            {
                throw new Exception($"Error in  loading the package {package} from the source {source}");
            }
        }

        private static async Task<Assembly> LoadAssemblies(string folderName, string package)
        {
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            //get the package assembly
            var assemblies = Directory.GetFiles(folderName,
                "*.dll", SearchOption.AllDirectories);

            if (assemblies is { Length: <= 0 })
                throw new Exception($"No assembly files found in the folder {folderName}");

            Assembly initialAssembly = null!;
            foreach (var assemblyPath in assemblies)
            {
                if (Path.GetFileName(assemblyPath).Equals($"{package}.dll", StringComparison.OrdinalIgnoreCase))
                    initialAssembly = Assembly.Load(await File.ReadAllBytesAsync(assemblyPath).ConfigureAwait(false));
                else
                    _ = Assembly.Load(await File.ReadAllBytesAsync(assemblyPath).ConfigureAwait(false));
            }

            return initialAssembly;
        }

        internal static async Task<Assembly> LoadAssemblies(this string assemblyPath)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var initialAssembly = Assembly.Load(await File.ReadAllBytesAsync(assemblyPath).ConfigureAwait(false));
            var referencedAssemblies = initialAssembly.GetReferencedAssemblies();
            if (referencedAssemblies is { Length: <= 0 })
                return initialAssembly;

            var assemblyDirectoryName = Path.GetDirectoryName(assemblyPath);

            if (string.IsNullOrEmpty(assemblyDirectoryName))
                return initialAssembly;

            string[] allAssemblies =
                Directory.GetFiles(assemblyDirectoryName, "*.dll", SearchOption.AllDirectories);

            foreach (var referencedAssembly in referencedAssemblies)
            {
                foreach (var assembly in allAssemblies)
                {
                    if (referencedAssembly.Name == Path.GetFileNameWithoutExtension(assembly))
                        Assembly.Load(await File.ReadAllBytesAsync(assembly).ConfigureAwait(false));
                }
            }

            return initialAssembly;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = ((AppDomain)sender).GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);
            return assembly!;
        }
    }
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
}
