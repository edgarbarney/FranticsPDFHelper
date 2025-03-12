using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Frantics_PDF_Helper.Utilities;
using Frantics_PDF_Helper.Windows;

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

			public static Language PlaceholderLanguage => new("English", "en-GB");
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

		public static Language defaultLanguage = Language.PlaceholderLanguage;
		public static Language currentLanguage = defaultLanguage;

		public const string languageDataPath = "Resources/Localisation";

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
		public static void InitLocalisation()
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

			if (!String.IsNullOrEmpty(Settings.Instance.ChosenLanguage))
			{
				var selectedLang = Settings.Instance.ChosenLanguage;
				SetCurrentLanguage(selectedLang);
			}
			else
			{
				SetCurrentLanguage(defaultLanguage.LanguageCode);
			}
		}

		public static List<Language> GetAvailableLanguages()
		{
			return AvailableLanguages;
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
		/// Check if this key exists in the localisation data.
		/// Used mainly for tooltips.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool DoesLocalisedKeyExist(string key)
		{
			if (MainLocalisationData.TryGetValue(currentLanguage.LanguageCode, out var langDict) && langDict.TryGetValue(key, out var value))
			{
				return true;
			}
			return false;
		}

		public static string GetLocalisedEnumString<T>(T enumValue)
		{
			string? enumName = string.Empty;

			if (enumValue != null)
			{
				enumName = Enum.GetName(typeof(T), enumValue);
			}

			string enumKey = $"{typeof(T).Name}.{enumName}";
			return GetLocalisedString(enumKey);
		}

		/*
		/// <summary>
		/// Sets the content and tooltip of a button based on its tag. Tag should be in the format "LocKey:'key'".
		/// </summary>
		/// 
		/// <param name="button">Button with tag to change content and tooltip of</param>
		/// <param name="tooltip">Whether to set the tooltip or not</param>
		public static void SetTaggedButtonContent(Button button, bool tooltip = true)
		{
			string? tagValue = button.Tag.ToString();

			if (tagValue == null)
			{
				//DialogueWindow.ShowDialogue("Error", "Button tag is problematic.", DialogueWindow.DialogueManner.Error);
				return;
			}

			button.Content = GetLocalisedString(StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey"));
			
			if (tooltip)
				button.ToolTip = GetLocalisedString(StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey") + ".Tooltip");
		}
		*/

		public static void LocaliseWindow(Window window)
		{
			string? tagValue;
			string keyValue;

			// Window title is always the app name.
			window.Title = GetLocalisedString("_AppName");

			foreach (var childControl in WPFControlUtilities.GetChildren((System.Windows.Media.Visual)window.Content))
			{
				if (childControl is ContentControl contentControl)
				{
					if (contentControl.Tag == null)
					{
						continue;
					}

					tagValue = contentControl.Tag.ToString();
					keyValue = StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey");

					if (keyValue != null)
					{
						contentControl.Content = GetLocalisedString(keyValue);
					}
					if (DoesLocalisedKeyExist(keyValue + ".Tooltip"))
					{
						contentControl.ToolTip = GetLocalisedString(keyValue + ".Tooltip");
					}
				}

				// ========================
				// Unused code examples, just in case if they are needed in the future.
				// ========================

				// We're using a general ContentControl check instead.
				// And SetTaggedButtonContent is deprecated.
				//
				//if (childControl is Button button)
				//{
				//	SetTaggedButtonContent(button, true);
				//}

				// This should be handled when the ComboBox is populated.
				//
				//else if (childControl is ComboBox comboBox)
				//{
				//	foreach (var item in comboBox.Items)
				//	{
				//		if (item is ComboBoxItem comboBoxItem)
				//		{
				//			tagValue = comboBoxItem.Tag.ToString();
				//			keyValue = StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey");
				//			if (keyValue != null)
				//			{
				//				comboBoxItem.Content = GetLocalisedString(keyValue);
				//			}
				//		}
				//	}
				//}

				// We're using a general ContentControl check instead.
				//
				//else if (childControl is Label label)
				//{
				//	tagValue = label.Tag.ToString();
				//	keyValue = StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey");
				//	if (keyValue != null)
				//	{
				//		label.Content = GetLocalisedString(keyValue);
				//	}
				//}
				//else if (childControl is CheckBox checkBox)
				//{
				//	tagValue = checkBox.Tag.ToString();
				//	keyValue = StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey");
				//	if (keyValue != null)
				//	{
				//		checkBox.Content = GetLocalisedString(keyValue);
				//	}
				//}

				// Not needed for now.
				// 
				//else if (childControl is TextBlock textBlock)
				//{
				//	tagValue = textBlock.Tag.ToString();
				//	keyValue = StringUtilities.GetXamlTagKeyValue(tagValue, "LocKey");
				//	if (keyValue != null)
				//	{
				//		textBlock.Text = GetLocalisedString(keyValue);
				//	}
				//	if (DoesLocalisedKeyExist(keyValue + ".Tooltip"))
				//	{
				//		textBlock.ToolTip = GetLocalisedString(keyValue + ".Tooltip");
				//	}
				//}
			}
		}

		public static void SetCurrentLanguage(string langCode)
		{
			if (AvailableLanguages.Any(lang => lang.IsLanguageCode(langCode)))
			{
				currentLanguage = AvailableLanguages.First(lang => lang.IsLanguageCode(langCode));
				Settings.Instance.ChosenLanguage = currentLanguage.LanguageCode;
			}
			else 
			{
				// Couldn't find a direct match. Try to match the parent language.
				// We assume the parent language is in the characters before the first hyphen.
				//
				// For example, "en-GB" will match "en-US", "en-CA", etc.
				// "tr-TR" will match "tr-AZ", "tr-IR", etc.
				// "bs-Latn-BA" will match "bs-Cyrl-BA", "bs-Latn-HR", etc.
				//
				// If no match is found, default to English.

				string parentLangCode = langCode.Split('-')[0];
				if (AvailableLanguages.Any(lang => lang.IsLanguageCode(parentLangCode)))
				{
					currentLanguage = AvailableLanguages.First(lang => lang.IsLanguageCode(parentLangCode));
					Settings.Instance.ChosenLanguage = currentLanguage.LanguageCode;
				}
				else
				{
					DialogueWindow.ShowDialogue("Error", "Failed to set language. Defaulting to English.", DialogueWindow.DialogueManner.Error);
					currentLanguage = defaultLanguage;
					Settings.Instance.ChosenLanguage = currentLanguage.LanguageCode;
				}
			}
		}

		public static void SetCurrentLanguageByName(string langName)
		{
			if (AvailableLanguages.Any(lang => lang.IsLanguageName(langName)))
			{
				string langCode = AvailableLanguages.First(lang => lang.IsLanguageName(langName)).LanguageCode;
				SetCurrentLanguage(langCode);
			}
			else
			{
				DialogueWindow.ShowDialogue("Error", "Failed to set language. Defaulting to English.", DialogueWindow.DialogueManner.Error);
				currentLanguage = defaultLanguage;
				Settings.Instance.ChosenLanguage = currentLanguage.LanguageCode;
			}
		}
	}
}
