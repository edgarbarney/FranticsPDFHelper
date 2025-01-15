using System.IO;
using System.Reflection;
using Newtonsoft.Json;

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
				RestartApp();
			}

			Instance.Reset();
		}

		public static void SaveSettings()
		{
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
			// Thanks to Bali C
			//ProcessStartInfo Info = new ProcessStartInfo();
			//Info.Arguments = "/C ping 127.0.0.1 -n 2 && \"" + Application.ExecutablePath + "\"";
			//Info.WindowStyle = ProcessWindowStyle.Hidden;
			//Info.CreateNoWindow = true;
			//Info.FileName = "cmd.exe";
			//Process.Start(Info);
			//Application.Current.Shutdown();


			System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
			System.Windows.Application.Current.Shutdown();
		}
	}
}
