using System;
using System.Text.RegularExpressions;

namespace Frantics_PDF_Helper.Utilities
{
	static class StringUtilities
	{
		/// <summary>
		/// A basic parser to extract a value from a key-value format that I made.
		/// </summary>
		/// 
		/// <example>
		/// "LocKey:'SelectPDFWindow.LoadPDFFile', AnotherXamlTag:13443, TagOfXaml:25.45643,";
		/// </example>
		/// 
		/// <param name="input">The string to parse</param>
		/// <param name="key">Key to look for</param>
		/// <returns>The corresponding value of the key</returns>
		public static string GetXamlTagKeyValue(string? input, string key)
		{
			if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key))
			{
				return ""; // Return empty string if input or key is null or empty
			}

			string pattern = $"\\b{Regex.Escape(key)}:(\\'[^']*\\'|[0-9.]+)";
			var match = Regex.Match(input, pattern);

			if (match.Success)
			{
				string value = match.Groups[1].Value;
				// Remove surrounding single quotes if present
				if (value.StartsWith("'") && value.EndsWith("'"))
				{
					value = value.Substring(1, value.Length - 2);
				}
				return value;
			}

			return ""; // Return empty string if the key is not found
		}
	}
}
