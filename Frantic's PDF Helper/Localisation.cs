using System.IO;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace Frantics_PDF_Helper
{
	static class Localisation
	{		
		// Should this be a class?
		/// <summary>
		/// Contains information about a language.
		/// Name and Code are the two main properties.
		/// </summary>
		public struct Language(string langName, string langCode)
		{
			private string languageName = langName;
			private string languageCode = langCode;

			public string LanguageName
			{
				readonly get { return languageName; }
				set { languageName = value; }
			}

			public string LanguageCode
			{
				readonly get { return languageCode; }
				set { languageCode = value; }
			}

			public readonly override bool Equals(object? obj)
			{
				if (obj != null && obj is Language language)
				{
					Language other = language;
					return other.LanguageCode == this.LanguageCode;
				}
				return false;
			}

			public readonly override int GetHashCode() => this.LanguageCode.GetHashCode();

			/// <summary>
			/// Checks if this language's code is identical to the given language code.
			/// </summary>
			/// <param name="langCode">Language Name to check for.</param>
			public readonly bool IsLanguageCode(string langCode) => langCode == this.LanguageCode;

			/// <summary>
			/// Checks if this language's name is identical to the given language name.
			/// </summary>
			/// <param name="langName">Language Code to check for.</param>
			public readonly bool IsLanguageName(string langName) => langName == this.LanguageName;

		}

		/// <summary>
		/// Contains available languages
		/// </summary>
		public static List<Language> AvailableLanguages =
		[
			// Example structure.
			// This will be populised by RefreshLocalistaion() method.
			new Language("English",	"en-GB"),
			new Language("Turkce",	"tr-TR"),
		];

		/// <summary>
		/// Contains key-value pairs for each localised string, as a dictionary of dictionaries.
		/// Every language has its own dictionary.
		/// </summary>
		///
		/// <remarks>
		/// Key: Language code
		/// Value: Dictionary of key-value pairs for that language.
		/// </remarks>
		public static Dictionary<string, Dictionary<string, string>> MainLocalisationData = [];

		public static Language defaultLanguage = new("English", "en-GB");
		public static Language currentLanguage = defaultLanguage;

		public const string languageDataPath = "Resources/Localisation";
		public const string selectedLanguageFile = "Resources/Localisation/SelectedLanguage.txt"; //Path.Combine(languageDataPath, "SelectedLanguage.txt");

		private static void AddNewLanguage(string langName, string langCode)
		{
			var newLang = new Language(langName, langCode);

			if (!AvailableLanguages.Contains(newLang))
			{
				AvailableLanguages.Add(newLang);
			}
		}

		private static void ClearLocalisationData()
		{
			AvailableLanguages.Clear();
			MainLocalisationData.Clear();
			currentLanguage = defaultLanguage;
		}

		// TODO: Should we make this called more clearly?
		public static void InıtLocalisation()
		{
			RefreshLocalistaion();
		}

		public static void RefreshLocalistaion()
		{
			ClearLocalisationData();

			// Read folder structure of the localisation files.
			// For each folder, read the JSON files.
			// For each JSON file, read the key-value pairs.
			// Add the key-value pairs to the dictionaries.

			foreach (string langCodeDir in Directory.GetDirectories(languageDataPath))
			{
				var files = Directory.GetFiles(langCodeDir, "*.json");
				var langCode = Path.GetFileName(langCodeDir); // Get the last part of the path.
				if (files.Length > 0)
				{
					foreach (string file in files)
					{
						string json = File.ReadAllText(file);
						Dictionary<string, string>? dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
						
						if (dict == null)
						{
							continue;
						}

						foreach (KeyValuePair<string, string> p in dict)
						{
							if (p.Key == "_LanguageName")
							{
								AddNewLanguage(p.Value, langCode);
							}
							//else
							//{
								if (!MainLocalisationData.ContainsKey(langCode))
								{
									MainLocalisationData.Add(langCode, []);
								}

								MainLocalisationData[langCode].Add(p.Key, p.Value);
							//}
						}
					}
				} 
			}

			if (File.Exists(selectedLanguageFile))
			{
				var selectedLang = File.ReadAllText(selectedLanguageFile);
				if (AvailableLanguages.Any(lang => lang.IsLanguageCode(selectedLang)))
				{
					currentLanguage = AvailableLanguages.First(lang => lang.IsLanguageCode(selectedLang));
				}
			}
			else
			{
				//var selectedLang = File.Create(selectedLangPath);
				File.WriteAllText(selectedLanguageFile, defaultLanguage.LanguageCode);
			}
		}

		public static string GetCurrentLanguageName()
		{
			return currentLanguage.LanguageName;
		}

		public static string GetCurrentLanguageCode()
		{
			return currentLanguage.LanguageCode;
		}

		public static string GetLocalisedString(string key)
		{
			//	if (MainLocalisationData.ContainsKey(currentLanguage.LanguageCode))
			//	{
			//		if (MainLocalisationData[currentLanguage.LanguageCode].ContainsKey(key))
			//		{
			//			return MainLocalisationData[currentLanguage.LanguageCode][key];
			//		}
			//	}
			//	
			//	return key;

			if (MainLocalisationData.TryGetValue(currentLanguage.LanguageCode, out var langDict) && langDict.TryGetValue(key, out var value))
			{
				return value;
			}

			// Get the last part of the placeholder phrase.
			int lastDotIndex = key.LastIndexOf('.');
			if (lastDotIndex != -1)
			{
				//return key.Substring(lastDotIndex + 1);
				return key[(lastDotIndex + 1)..];
			}
			return key;
		}

		/// <summary>
		/// Sets the content of a button based on its tag. Tag should be in the format "LocKey:'key'".
		/// </summary>
		/// 
		/// <param name="button">Button with tag to change content of</param>
		public static void SetTaggedButtonContent(Button button)
		{
			string? tagValue = button.Tag.ToString();

			if (tagValue == null)
			{
				// TODO: Log error, or throw exception?
				return;
			}

			button.Content = GetLocalisedString(Utilities.StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey"));
		}

		public static void SetCurrentLanguage(string langCode)
		{
			if (AvailableLanguages.Any(lang => lang.IsLanguageCode(langCode)))
			{
				currentLanguage = AvailableLanguages.First(lang => lang.IsLanguageCode(langCode));
				File.WriteAllText(selectedLanguageFile, langCode);
			}
		}
	}
}
