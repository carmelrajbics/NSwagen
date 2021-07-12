using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NSwagen.Core.Events
{
    public sealed class StatusEventArgs<TType> : EventArgs
        where TType : Enum
    {
        private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>(
            StringComparer.OrdinalIgnoreCase);

        public StatusEventArgs(TType statusType, object? metadata = null, string? message = null)
        {
            StatusType = statusType;

            if (metadata != null)
            {
                PropertyInfo[] properties = metadata.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                {
                    if (!property.CanRead)
                        continue;
                    if (property.GetIndexParameters().Length > 0)
                        continue;
                    object value = property.GetValue(metadata) !;
                    _metadata.Add(property.Name, value);
                }
            }

            Message = message is null ? message : Patterns.PlaceholderPattern.Replace(message, match =>
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                if (_metadata.TryGetValue(match.Groups[1].Value, out object obj))
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                    return obj.ToString();
#pragma warning restore CS8603 // Possible null reference return.
                return match.Value;
            });
        }

        public TType StatusType { get; }

        public string? Message { get; }
    }

    internal static class Patterns
    {
        internal static readonly Regex PlaceholderPattern = new Regex(@"\{(\w+)\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
