using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Reflection;

using DispatcherTimer = System.Windows.Threading.DispatcherTimer;

namespace Frantics_PDF_Helper.Windows
{
	/// <summary>
	/// Interaction logic for SplashWindow.xaml
	/// </summary>
	public partial class SplashWindow : Window
	{
		public void OpenSelectPDFWindow(object? sender, EventArgs e)
		{
			SelectPDFWindow selectPDFWindow = new();
			selectPDFWindow.Show();
			this.Close();

			DispatcherTimer dispatcherTimer = (sender == null) ? new() : (DispatcherTimer)sender;
			dispatcherTimer.Stop();
		}

		public SplashWindow()
		{
			InitializeComponent();

			Settings.LoadSettings();

			this.Title = Localisation.GetLocalisedString("_AppName");

			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "Frantics_PDF_Helper.Resources.Splash.png";

			Stream? stream = assembly.GetManifestResourceStream(resourceName);

			if (stream == null)
			{
				throw new Exception("Could not load resource " + resourceName);
			}

			using (StreamReader reader = new StreamReader(stream))
			{
				splashImage.Source = BitmapFrame.Create(reader.BaseStream);
			}

			DispatcherTimer dispatcherTimer = new();
			dispatcherTimer.Tick += new EventHandler(OpenSelectPDFWindow);
			dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
			dispatcherTimer.Start();

			Localisation.InitLocalisation();
		}
	}
}
