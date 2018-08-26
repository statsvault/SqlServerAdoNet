using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Common library helper functions.
    /// </summary>
    internal static class CommonHelper
    {
        /// <summary>
        /// Case-insensitive string replacement.
        /// http://stackoverflow.com/questions/6275980/string-replace-ignorin
        /// </summary>
        /// <param name="stringToSearch">String to search in.</param>
        /// <param name="oldValue">String value to search for.</param>
        /// <param name="newValue">Replacement values.</param>
        /// <returns>string</returns>
        public static string ReplaceInsensitive(this string stringToSearch, string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(stringToSearch) && !string.IsNullOrEmpty(oldValue) && newValue != null)
            {
                return Regex.Replace(stringToSearch, Regex.Escape(oldValue), newValue.Replace("$", "$$"), RegexOptions.IgnoreCase); 
            }
            return stringToSearch;
        }

        /// <summary>
        /// Remove the specified text at the end of a string.
        /// http://stackoverflow.com/questions/7170909/trim-string-from-the-end-of-a-string-in-net-why-is-this-missing
        /// </summary>
        /// <param name="source">String to remove text from.</param>
        /// <param name="value">Text to remove.</param>
        /// <returns>string</returns>
        public static string RemoveFromEnd(this string source, string value)
        {
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(value))
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return source.EndsWith(value, comparison) ? source.Remove(source.LastIndexOf(value, comparison)) : source;
            }
            return source;
        }

        /// <summary>
        /// Convert value to decimal.
        /// </summary>
        /// <param name="value">Value to convert to decimal.</param>
        /// <returns>Nullable decimal</returns>
        public static decimal? ToDecimal(this object value)
        {
            if (value != null && 
                value != DBNull.Value && 
                decimal.TryParse(value.ToString(), out decimal dec))
            {
                return dec;
            }
            return null;
        }
    }
}
