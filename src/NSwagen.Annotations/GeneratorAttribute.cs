using System;

namespace NSwagen.Annotations
{
    /// <summary>
    /// Specifies the client generators and tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GeneratorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorAttribute"/> class.
        /// </summary>
        /// <param name="type">The client generator type.</param>
        /// <param name="generatorMethod">The client generator method name.</param>
        /// <param name="tags">Tags to identify the client generator.</param>
        public GeneratorAttribute(Type type, string generatorMethod, params string[] tags)
        {
            Type = type;
            Tags = tags;
            GeneratorMethod = generatorMethod;
        }

        /// <summary>
        /// Gets the type of client generator.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the tags to identify the client generator.
        /// </summary>
        public string[] Tags { get; }

        public string GeneratorMethod { get; }
    }
}
