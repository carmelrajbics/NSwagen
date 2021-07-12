using System;

namespace NSwagen.Annotations
{
    /// <summary>
    ///  Specifies the property which required to be passed for client generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class GeneratorPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorPropertyAttribute"/> class.
        /// </summary>
        /// <param name="description">Description of the generator property.</param>
        /// <param name="defaultValue">Default value of the generator property.</param>
        /// <param name="required">Is required boolean value.</param>
        /// <param name="name">The property name.</param>
        /// <param name="dataType">Data type of generator property.</param>
        public GeneratorPropertyAttribute(string description, string defaultValue, bool required, string? name = null, string? dataType = null)
        {
            Description = description;
            DefaultValue = defaultValue;
            Required = required;
            Name = name;
            DataType = dataType;
        }

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether property is required.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the data type of the property.
        /// </summary>
        public string? DataType { get; }
    }
}
