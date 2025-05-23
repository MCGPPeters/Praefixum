using System;

namespace Praefixum
{
    /// <summary>
    /// Format options for UniqueId generation
    /// </summary>
    public enum UniqueIdFormat
    {
        /// <summary>
        /// 16-character hexadecimal string (default)
        /// </summary>
        Hex16 = 0,
        
        /// <summary>
        /// 32-character hexadecimal string (full MD5 hash)
        /// </summary>
        Hex32 = 1,
        
        /// <summary>
        /// Standard GUID format with dashes
        /// </summary>
        Guid = 2,
          /// <summary>
        /// 8-character hexadecimal string (short format)
        /// </summary>
        Hex8 = 3,
        
        /// <summary>
        /// Compact format optimized for HTML5 element IDs: 
        /// 6-character string that starts with a letter, followed by lowercase a-z, 0-9, hyphens (-), or underscores (_)
        /// Fully compliant with HTML5 ID specifications
        /// </summary>
        HtmlId = 4
    }

    /// <summary>
    /// Attribute to mark a parameter that will receive a unique ID at compile time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class UniqueIdAttribute : Attribute
    {
        /// <summary>
        /// The format of the generated unique ID
        /// </summary>
        public UniqueIdFormat Format { get; }

        /// <summary>
        /// Creates a new UniqueIdAttribute with the default format (Hex16)
        /// </summary>
        public UniqueIdAttribute()
        {
            Format = UniqueIdFormat.Hex16;
        }

        /// <summary>
        /// Creates a new UniqueIdAttribute with the specified format
        /// </summary>
        /// <param name="format">The format to use for the generated ID</param>
        public UniqueIdAttribute(UniqueIdFormat format)
        {
            Format = format;
        }
    }
}
