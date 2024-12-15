using System;
using System.IO;
using System.Security.Cryptography;


namespace Frantics_PDF_Helper.Utilities
{
	static class FileUtilities
	{
		/// <summary>
		/// Gets the MD5 hash of a file.
		/// </summary>
		public static string GetMD5Hash(string filename)
		{
			var md5 = MD5.Create();
			var stream = File.OpenRead(filename);
			return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
		}
	}
}
