//using System;
//using System.Collections.Generic;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using NSwagen.Core.Models;

//namespace NSwagen.Cli
//{
//    public class ConfigurationConverter : JsonConverter<GeneratorConfiguration>
//    {
//        public override GeneratorConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            var configuration = new GeneratorConfiguration();
//            while (reader.Read())
//            {
//                if (reader.TokenType == JsonTokenType.EndObject)
//                {
//                    return configuration;
//                }

//                if (reader.TokenType == JsonTokenType.PropertyName)
//                {
//                    var propertyName = reader.GetString();
//                    reader.Read();
//                    switch (propertyName)
//                    {
//                        case nameof(configuration.Generator.Assembly):
//                            var assembly = reader.GetString();
//                            configuration.Generator.Assembly = assembly;
//                            break;
//                        case nameof(configuration.Generator.Package):
//                            var package = reader.GetString();
//                            configuration.Generator.Package = package;
//                            break;
//                    }
//                }
//            }

//            throw new JsonException();
//        }

//        public override void Write(Utf8JsonWriter writer, GeneratorConfiguration value, JsonSerializerOptions options)
//        {
//            if (writer == null)
//                return;

//            if (value == null)
//                return;

//            writer!.WriteStartObject();

//            writer.WriteString(nameof(GeneratorConfiguration.Swagger), value.Swagger);
//            writer.WriteString(nameof(GeneratorConfiguration.Output), value.Output);

//            writer.WriteStartObject(nameof(GeneratorConfiguration.Generator));

//            if (string.IsNullOrEmpty(value.Generator.Package) && string.IsNullOrEmpty(value.Generator.Assembly))
//            {
//                writer.WriteString(nameof(GeneratorConfiguration.Generator.Package), "<package name>");
//                writer.WriteString(nameof(GeneratorConfiguration.Generator.Assembly), "<assembly file path>");
//            }
//            else if (!string.IsNullOrEmpty(value.Generator.Package))
//            {
//                writer.WriteString(nameof(GeneratorConfiguration.Generator.Package), value.Generator.Package);
//                writer.WriteString(nameof(GeneratorConfiguration.Generator.Source), value.Generator.Source);
//            }
//            else if (!string.IsNullOrEmpty(value.Generator.Assembly))
//            {
//                writer.WriteString(nameof(GeneratorConfiguration.Generator.Assembly), value.Generator.Assembly);
//            }

//            writer.WriteString(nameof(GeneratorConfiguration.Generator.Name), value.Generator.Name);

//            serializer.Serialize(writer, new Dictionary<string, object> { { value.Generator.Properties.Keys, value.Generator.Properties.Values } });

//            if (value.Generator.Properties is { Count: > 0 })
//            {
//                writer.WriteStartObject(nameof(GeneratorConfiguration.Generator.Properties));
//                foreach (var (key, o) in value.Generator.Properties)
//                {
//                    writer.WriteString(key, )
//                }
//                writer.WriteEndObject();
//            }

//            writer.WriteEndObject();
//            writer.WriteEndObject();
//        }
//    }
//}
