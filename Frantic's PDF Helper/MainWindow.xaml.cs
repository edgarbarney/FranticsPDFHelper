using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

using PdfiumViewer;
using SkiaSharp;

using Colour = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Resolution = Frantics_PDF_Helper.Utilities.Resolution;
using PaperType = Frantics_PDF_Helper.Utilities.PaperType;


namespace Frantics_PDF_Helper
{
	enum ImageFormat
	{
		Png,
		Webp
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const string exportFolderDir = "Cache/Files/";

		// Default paper is A4 (210x297mm)
		private PaperType basePaperKind = PaperType.A4;

		private PdfDocument? pdfDocument = null;
		private PdfRenderer pdfRenderer = new();
		private string pdfHash = "";

		private ImageFormat exportImageFormat = ImageFormat.Png;

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

		static readonly Brush cutModeOnBrush = new SolidColorBrush(Colour.FromRgb(153, 0, 0));  // #900
		static readonly Brush cutModeOffBrush = new SolidColorBrush(Colour.FromRgb(0, 0, 153)); // #009

		public MainWindow()
		{
			InitializeComponent();

			// Save image as webp if Windows 10 version 17763 or later
			if (OperatingSystem.IsOSPlatformVersionAtLeast("Windows", 10, 0, 17763))
			{
				exportImageFormat = ImageFormat.Webp;
			}
			else
			{
				exportImageFormat = ImageFormat.Png;
			}

			this.Title = Localisation.GetLocalisedString("_AppName");

			cutCompleteButton.IsEnabled = false;
			cutModeButton.Foreground = cutModeOffBrush;
		}

		// ========================
		// Button Events
		// ========================

		private void CutModeButton_Click(object sender, RoutedEventArgs e)
		{
			SetCutMode(!cutMode);
		}

		private void CutCompleteButton_Click(object sender, RoutedEventArgs e)
		{
			SetCutMode(false);
			cutCompleteButton.IsEnabled = false;
		}

		private void CloseMainWindowButton_Click(object sender, RoutedEventArgs e)
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

		public void LoadPDF(string path)
		{
			pdfDocument = PdfDocument.Load(path);
			pdfRenderer.Load(pdfDocument);
			pdfHash = Utilities.FileUtilities.GetMD5Hash(path);

			Directory.CreateDirectory(exportFolderDir + pdfHash);
			//File.WriteAllText($"{exportFolderDir}{Path.GetFileName(path)}/hash.txt", );
		}

		public void ExportFullPDF()
		{
			if (pdfDocument == null)
			{
				throw new System.Exception("No PDF document loaded.");
			}

			Resolution resolution = Resolution.PaperSize(1100, 1100, basePaperKind);

			// Just in case the folder doesn't exist
			Directory.CreateDirectory(exportFolderDir + pdfHash);

			for (int page = 0; page < pdfDocument.PageCount; page++)
			{
				var img = pdfDocument.Render(page, resolution.Width, resolution.Height, 96, 96, false);
				ExportImage(img, $"{exportFolderDir}{pdfHash}/{page}");
			}
		}

		public void ExportImage(System.Drawing.Image image, string name)
		{
			switch (exportImageFormat)
			{
				case ImageFormat.Png:
					image.Save($"{name}.png", System.Drawing.Imaging.ImageFormat.Png);
					break;
				case ImageFormat.Webp:
					MemoryStream tempStream = new MemoryStream();
					image.Save(tempStream, System.Drawing.Imaging.ImageFormat.Bmp);
					tempStream.Seek(0, SeekOrigin.Begin); // Reset stream position
					var skBitmap = SKBitmap.Decode(tempStream);
					var webpData = skBitmap.Encode(SKEncodedImageFormat.Webp, 100);
					File.WriteAllBytes($"{name}.webp", webpData.ToArray());
					break;
			}
		}	
	}
}
