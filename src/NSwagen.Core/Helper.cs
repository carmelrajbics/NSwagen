using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NSwagen.Annotations;
using NSwagen.Core.Models;

namespace NSwagen.Core
{
    public static class Helper
    {
        internal static IEnumerable<GeneratorProperty> GetGeneratorProperties(Assembly assembly)
        {
            IEnumerable<Type> types = assembly.GetExportedTypes();

            List<GeneratorProperty> generatorProperties = new List<GeneratorProperty>();

            foreach (var type in types.Where(t => t.IsClass))
            {
                var attributes = type.GetCustomAttributes(typeof(GeneratorPropertyAttribute), true);

                if (attributes is { Length: <= 0 })
                    continue;

                generatorProperties.AddRange(from GeneratorPropertyAttribute attr in attributes
                                             where generatorProperties.FirstOrDefault(p => p.Name == (attr.Name ?? type.Name)) == null
                                             select new GeneratorProperty
                                             {
                                                 Name = attr.Name ?? type.Name,
                                                 Required = attr.Required,
                                                 DataType = attr.DataType ?? type.FullName,
                                                 DefaultValue = attr.DefaultValue,
                                                 Description = attr.Description,
                                             });

                generatorProperties.AddRange(from propertyInfo in type.GetProperties()
                                             let propAttributes = propertyInfo.GetCustomAttributes(typeof(GeneratorPropertyAttribute), true)
                                             from GeneratorPropertyAttribute propAttribute in propAttributes
                                             where generatorProperties.FirstOrDefault(p => p.Name == (propAttribute.Name ?? propertyInfo.Name)) == null
                                             select new GeneratorProperty
                                             {
                                                 Name = propAttribute.Name ?? propertyInfo.Name,
                                                 Required = propAttribute.Required,
                                                 DataType = propAttribute.DataType ?? propertyInfo.PropertyType.FullName,
                                                 DefaultValue = propAttribute.DefaultValue,
                                                 Description = propAttribute.Description,
                                             });
            }

            return generatorProperties;
        }

        internal static void PackageCleanup(string packagePath, string extractedPath)
        {
            //Delete the nuget package
            File.Delete(packagePath);
            Directory.Delete(extractedPath, true);
        }
    }
}
