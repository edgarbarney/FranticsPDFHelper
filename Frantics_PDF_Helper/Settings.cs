using System.IO;
using System.Reflection;
using Newtonsoft.Json;

using Frantics_PDF_Helper.Windows;

namespace Frantics_PDF_Helper
{
	public enum ImageFormat
	{
		Png,
		Webp
	}

	public enum PDFCacheMode
	{
		NoCache,        // Don't cache anything
		OnPageChange,   // Cache only the current page
		FullPDF,        // Cache the full PDF
	}

	class Settings
	{
		private static Settings instance = new();
		public static Settings Instance
		{
			get
			{
				return instance;
			}
			private set
			{
				instance = value;
			}
		}

		public const string settingsPath = "Cache/Settings.json";

		public ImageFormat ExportFormat { get; set; } = ImageFormat.Png;
		public PDFCacheMode CacheMode { get; set; } = PDFCacheMode.NoCache;
		public string LastPDFPath { get; set; } = "";
		public bool IsFirstRun { get; set; } = true;

		public void Reset()
		{
			ExportFormat = ImageFormat.Png;
			CacheMode = PDFCacheMode.NoCache;
			LastPDFPath = "";
			Instance.IsFirstRun = false;
		}

		public static void ResetSettings(bool restart)
		{
			if (restart)
			{
				if (File.Exists(settingsPath))
				{
					File.Delete(settingsPath);
				}
				else
				{
					Instance.Reset();
					SaveSettings();
				}
			}

			Instance.Reset();
		}

		public static void SaveSettings()
		{
			var directory = Path.GetDirectoryName(settingsPath);
			// Create directory
			if (directory == null)
			{
				DialogueWindow.ShowDialogue("Error", Localisation.GetLocalisedString("Settings.NullDirectory"), DialogueWindow.DialogueManner.Error);
				return;
			}

			Directory.CreateDirectory(directory);
			File.WriteAllText(settingsPath, JsonConvert.SerializeObject(Instance));
		}

		public static void LoadSettings()
		{
			if (!File.Exists(settingsPath))
			{
				Instance.IsFirstRun = true;
				return;
			}
			Instance.IsFirstRun = false;

			string jsonData = File.ReadAllText(settingsPath);
			Settings? temp = JsonConvert.DeserializeObject<Settings>(jsonData);

			if (temp != null)
			{
				Instance = temp;
				
			}
			//else
			//{
			//	Instance = new();
			//}
		}

		public static void RestartApp()
		{
			var appPath = Environment.ProcessPath;
			if (appPath != null)
			{
				System.Diagnostics.Process.Start(appPath);
			}
			System.Windows.Application.Current.Shutdown();
		}
	}
}
