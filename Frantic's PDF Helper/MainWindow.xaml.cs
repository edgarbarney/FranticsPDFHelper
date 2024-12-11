using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageMagick;

namespace Frantics_PDF_Helper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		// Default paper is A4 (210x297mm)
		private int paperBaseHeight = 297;
		private int paperBaseWidth = 210;

		/// <summary>
		/// Are we currenly trying to crop?
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// True: Cut mode, mouse/drag drag higlights area to crop.
		/// </para>
		/// <para>
		/// False: Normal mode, mouse/touch drag moves the image.
		/// </para>
		/// </remarks>
		private bool cutMode = false;

		static readonly Brush cutModeOnBrush = new SolidColorBrush(Color.FromRgb(153, 0, 0));  // #900
		static readonly Brush cutModeOffBrush = new SolidColorBrush(Color.FromRgb(0, 0, 153)); // #009

		public MainWindow()
		{
			InitializeComponent();

			this.Title = Localisation.GetLocalisedString("_AppName");

			cutCompleteButton.IsEnabled = false;
			cutModeButton.Foreground = cutModeOffBrush;
		}

		// ========================
		// Button Events
		// ========================

		private void CutModeButton(object sender, RoutedEventArgs e)
		{
			SetCutMode(!cutMode);
		}

		private void CutCompleteButton(object sender, RoutedEventArgs e)
		{
			SetCutMode(false);
			cutCompleteButton.IsEnabled = false;
		}

		private void CloseMainWindowButton(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		// ========================
		// Button Events End
		// ========================

		private void SetCutMode(bool mode)
		{
			cutMode = mode;
			cutModeButton.Foreground = cutMode ? cutModeOnBrush : cutModeOffBrush;
		}

		private void CloseMainWindow()
		{
			Application.Current.Shutdown();
		}

		
	}
}