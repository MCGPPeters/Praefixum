using System;

namespace UniqueIdGenerator
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
        /// Compact format optimized for HTML element IDs: 
        /// 4-character alphanumeric string (lowercase a-z, 0-9)
        /// Always starts with a letter (HTML ID requirement)
        /// Uses an optimized algorithm to maximize uniqueness in just 4 characters
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
