using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NSwag;
using NSwagen.Annotations;
using NSwagen.Core.Events;
using NSwagen.Core.Models;

namespace NSwagen.Core
{
    public sealed class ClientGenerator
    {
#pragma warning disable S3264 // Events should be invoked
        public event EventHandler<StatusEventArgs<StatusEvents>> OnStatus = null!;
#pragma warning restore S3264 // Events should be invoked

        private readonly GeneratorConfiguration _configuration;

        public ClientGenerator(GeneratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Generate()
        {
            var clientCode = await GenerateClientCode().ConfigureAwait(false);

            await File.WriteAllTextAsync(_configuration.Output!, clientCode?.ToString()).ConfigureAwait(false);
        }

        public async Task<List<GeneratorConfiguration>> GetGeneratorInfo()
        {
            List<GeneratorConfiguration> configurations = new ();
            var (assembly, packagePath, extractedPackagePath) = await LoadAssembly().ConfigureAwait(false);

            //Get the generators
            var generatorAttributes = assembly.GetCustomAttributes<GeneratorAttribute>().ToList();
            if (generatorAttributes is { Count: > 0 })
            {
                foreach (var attribute in generatorAttributes)
                {
                    GeneratorConfiguration configuration = new ();
                    OnStatus.Fire(StatusEvents.Generate,
                        new { Generator = attribute.Type },
                        $"Identifying generator {attribute.Type.ToString()} information..");

                    configuration.Generator = new Generator()
                    {
                        Name = attribute.Type.ToString(),
                        Tags = attribute.Tags,
                        GeneratorProperties = Helper.GetGeneratorProperties(assembly),
                    };

                    configurations.Add(configuration);
                }
            }

            //Delete the package and extracted content
            if (!string.IsNullOrEmpty(packagePath) && !string.IsNullOrEmpty(extractedPackagePath))
                Helper.PackageCleanup(packagePath, extractedPackagePath);

            return configurations;
        }

        private async Task<object> GenerateClientCode()
        {
            OnStatus.Fire(StatusEvents.Generate,
                new { Swagger = _configuration.Swagger, Package = _configuration.Generator.Package, Source = _configuration.Generator.Source, Assembly = _configuration.Generator.Assembly },
                $"Processing swagger document {_configuration.Swagger}..");

            var swaggerDocument = await _configuration.Swagger.ReadSwaggerDocumentAsync().ConfigureAwait(false);

            var (assembly, packagePath, extractedPackagePath) = await LoadAssembly().ConfigureAwait(false);

            OnStatus.Fire(StatusEvents.Generate,
                new { GeneratorName = _configuration.Generator.Name },
                $"Identifying generator {_configuration.Generator.Name} information..");

            //Get the client generators from the attributes
            var generatorAttribute = GetGenerator(assembly, _configuration.Generator.Name, _configuration.Generator.Discover);

            if (generatorAttribute is null)
                throw new Exception($"Matching generator {_configuration.Generator.Name} not found.");

            Type? type = assembly.GetType(generatorAttribute.Type.FullName ?? generatorAttribute.Type.Name);
            if (type is null)
            {
                throw new Exception(
                    $"Generator '{generatorAttribute.Type.FullName}' not found in the assembly '{assembly.FullName}'");
            }

            var methodInfo = type?.GetMethods().Single(
                m =>
                    m.Name == generatorAttribute.GeneratorMethod &&
                    m.GetParameters().Length == 0);

            if (methodInfo is null)
                throw new Exception("No such method exists.");

            //Validate required properties passed
            OnStatus.Fire(StatusEvents.Generate,
                new { },
                $"Validating required properties..");

            var generatorProperties = Helper.GetGeneratorProperties(assembly);
            var missingRequiredProperties = generatorProperties
                .Where(generatorProperty => generatorProperty.Required &&
                                            !_configuration.Generator.Properties!.ContainsKey(generatorProperty.Name))
                .Select(_ => _.Name).ToList();

            if (missingRequiredProperties is { Count: > 0 })
            {
                throw new Exception(
                    $"Required properties needs to be specified for the generator '{generatorAttribute.Type.Name}' : '{string.Join(",", missingRequiredProperties)}'.");
            }

            //Validate required properties passed
            OnStatus.Fire(StatusEvents.Generate,
                new { GeneratorName = _configuration.Generator.Name },
                $"Invoking the generator..");

            var generatorInstance = CreateGeneratorInstance(type!, swaggerDocument);
            var generatedClient = methodInfo.Invoke(generatorInstance, null) ??
                         throw new Exception($"Error in invoking the method {methodInfo}");

            //Delete the package and extracted content
            if (!string.IsNullOrEmpty(packagePath) && !string.IsNullOrEmpty(extractedPackagePath))
                Helper.PackageCleanup(packagePath, extractedPackagePath);

            OnStatus.Fire(StatusEvents.Generate,
                new { },
                $"Successfully generated the client code..");

            return generatedClient;
        }

        private async Task<(Assembly, string?, string?)> LoadAssembly()
        {
            try
            {
                return string.IsNullOrEmpty(_configuration.Generator.Package)
                    ? (await _configuration.Generator.Assembly!.LoadAssemblies().ConfigureAwait(false), default, default)
                    : await _configuration.Generator.Package.ExtractNugetPackage(_configuration.Generator.Source)
                        .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new Exception("Error in loading package/assembly. Provide valid details.", e);
            }
        }

        private static GeneratorAttribute? GetGenerator(Assembly assembly, string? name, bool discover)
        {
            var generatorAttributes = assembly.GetCustomAttributes<GeneratorAttribute>().ToList();

            if (generatorAttributes is { Count: > 0 })
            {
                foreach (var generatorAttribute in generatorAttributes)
                {
                    if (generatorAttribute is { } && !string.IsNullOrEmpty(name) && (generatorAttribute.Type.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        generatorAttribute.Type.FullName!.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        generatorAttribute.Tags.Contains(name)))
                        return generatorAttribute;

                    //return (generatorAttribute.Type.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                    //        generatorAttribute.Type.FullName!.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                    //        generatorAttribute.Tags.Contains(name))
                    //    ? generatorAttribute
                    //    : throw new Exception($"Matching generator {name} not found.");

                    if (generatorAttribute is null && !discover)
                        throw new Exception("Generator cannot be found. Please enable the option as discover.");
                    else
                    {
                        //TODO - Discover without any attributes
                    }
                }
            }

            return default!;
        }

        private object CreateGeneratorInstance(Type type, OpenApiDocument swaggerDocument)
        {
            var constructorInfo = type.GetConstructors().FirstOrDefault();
            if (constructorInfo is null)
                throw new Exception($"Not able to create an instance of the type {type}");

            var parameterInfos = constructorInfo.GetParameters();
            var constructorParameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                //if (parameterInfos[i].ParameterType.Name == nameof(OpenApiDocument))
                if (parameterInfos[i] is { ParameterType: { Name: nameof(OpenApiDocument) } })
                {
                    constructorParameters[i] = swaggerDocument;
                }
                else
                {
                    if (!parameterInfos[i].ParameterType.IsClass || parameterInfos[i].ParameterType == typeof(string) ||
                        parameterInfos[i].ParameterType.IsPrimitive)
                        continue;

#pragma warning disable CS8601 // Possible null reference assignment.
                    constructorParameters[i] = Activator.CreateInstance(parameterInfos[i].ParameterType);
#pragma warning restore CS8601 // Possible null reference assignment.
                    if (_configuration.Generator.Properties is { Count: > 0 })
                        RecursivePropertyMapping(constructorParameters[i], _configuration.Generator.Properties);
                }
            }

            return Activator.CreateInstance(type, constructorParameters) ?? throw new Exception($"Error in creating generator {type} instance");
        }

        private static void RecursivePropertyMapping(object instance, Dictionary<string, object> dataDictionary)
        {
            foreach (var (key, value) in dataDictionary)
            {
                foreach (var prop in instance.GetType().GetProperties())
                {
                    //if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !prop.PropertyType.IsArray)
                    if (prop.PropertyType is { IsClass: true, IsArray: false } && prop.PropertyType != typeof(string))
                    {
                        var innerPropInstance = prop.GetValue(instance, null);
                        var matchProperty = innerPropInstance?.GetType().GetProperty(key);

                        if (matchProperty is { CanWrite: true })
                            matchProperty.SetValue(innerPropInstance, Convert.ChangeType(value.ToString(), Type.GetType(matchProperty.PropertyType.FullName ?? matchProperty.PropertyType.Name) !, CultureInfo.InvariantCulture));

                        //if (matchProperty != null && matchProperty.CanWrite)
                        //    matchProperty.SetValue(innerPropInstance, Convert.ChangeType(value.ToString(), Type.GetType(matchProperty.PropertyType.FullName)));
                    }

                    if (key == prop.Name)
                    {
                        prop.SetValue(instance, Convert.ChangeType(value.ToString(), Type.GetType(prop.PropertyType.FullName ?? prop.PropertyType.Name) !, CultureInfo.InvariantCulture));
                    }
                }
            }
        }
    }
}
