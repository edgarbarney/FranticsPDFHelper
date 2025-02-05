using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

using PdfiumViewer;
using Frantics_PDF_Helper.Utilities;

using Colour = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Resolution = Frantics_PDF_Helper.Utilities.Resolution;
using PaperType = Frantics_PDF_Helper.Utilities.PaperType;

namespace Frantics_PDF_Helper.Windows
{
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

		public int CurrentPage { get; private set; } = 0;
		public int TotalPages { get; private set; } = 0;

		private static readonly Brush cutModeOnBrush = new SolidColorBrush(Colour.FromRgb(153, 0, 0));          // #900
		private static readonly Brush cutModeOffBrush = new SolidColorBrush(Colour.FromRgb(0, 0, 153));			// #009
		private static readonly Brush cutCompleteOnBrush = new SolidColorBrush(Colour.FromRgb(0, 102, 0));      // #060
		private static readonly Brush cutCompleteOffBrush = new SolidColorBrush(Colour.FromRgb(102, 102, 102)); // #666

		private static readonly Brush dragRectFillBrush = new SolidColorBrush(Colour.FromArgb(64, 0, 0, 102));
		private static readonly Brush dragRectStrokeBrush = new SolidColorBrush(Colour.FromArgb(128, 0, 0, 102));

		private static readonly double defaultPageMargin = 128.0;

		private bool isCanvasInitialised = false;
		private bool isMouseDragging = false;
		private Point mouseDragStartPoint;
		private Point mouseDragEndPoint;
		private Point mouseDragAnchorPoint; // Starting offset

		public MainWindow()
		{
			InitializeComponent();

			this.Title = Localisation.GetLocalisedString("_AppName");

			mainPaper.MouseLeftButtonDown += MainPaper_MouseLeftButtonDown;
			mainPaper.MouseLeftButtonUp += MainPaper_MouseLeftButtonUp;
			mainPaper.MouseMove += MainPaper_MouseMove;

			// For the initial centring of the main paper
			mainCanvas.Loaded += InitialLayoutUpdate;

			SetCutMode(false);
		}

		// ========================
		// Control Events
		// ========================

		private async void InitialLayoutUpdate(object? sender, EventArgs e)
		{
			// TODO: Centre the paper on the canvas when the canvas is first loaded.
			// TODO: Find a cleaner way to do this.
			//
			// When "centring" the paper for the first time, it doesn't seem to work.
			// Centring function can't get the proper size of the canvas.
			// Canvas size, window size and screen size are all wrong.
			// Results are "1536x864" for a 1920x1080 screen.
			// So, the paper is not centred properly.
			// Tried disabling Scaling settings on Windows, but it didn't help.
			// The issue is not present after this "loaded" event.
			// 
			// This is a workaround to fix the issue.
			// But it is a bit hacky and not the best solution.
			// We should find a better way to centre the paper.

			CentreMainPaper();
			await Task.Delay(10);
			CentreMainPaper();

			isCanvasInitialised = true;
			mainCanvas.Loaded -= InitialLayoutUpdate;
		}

		private void CutModeButton_Click(object sender, RoutedEventArgs e)
		{
			SetCutMode(!cutMode);
		}

		private void CutCompleteButton_Click(object sender, RoutedEventArgs e)
		{
			SetCutMode(false);
		}

		private void CloseMainWindowButton_Click(object sender, RoutedEventArgs e)
		{
			if(DialogueWindow.ShowDialogue(Title, Localisation.GetLocalisedString("Generic.CloseAppQuestion")))
			{
				CloseMainWindow();
			}
		}

		private void PageNumberButton_Click(object sender, RoutedEventArgs e)
		{
			var diaRet = DialogueWindow.ShowInputDialogue(Title, Localisation.GetLocalisedString("MainWindow.EnterPageNumber"));

			if (!String.IsNullOrEmpty(diaRet) && Int32.TryParse(diaRet, out int targetPage) && targetPage > 0 && targetPage <= TotalPages)
			{
				ShowPage(--targetPage);
			}

			//SetPageNumberButtonLabel();
		}

		private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage > 0)
			{
				ShowPage(CurrentPage - 1);
			}
		}

		private void NextPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage < TotalPages - 1)
			{
				ShowPage(CurrentPage + 1);
			}
		}

		private void MainPaper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			isMouseDragging = true;
			mouseDragStartPoint = e.GetPosition(this);

			mouseDragAnchorPoint.X = Canvas.GetLeft(mainPaper);
			mouseDragAnchorPoint.Y = Canvas.GetTop(mainPaper);
		}

		private void MainPaper_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			isMouseDragging = false;
		}

		private void MainPaper_MouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseDragging && e.LeftButton == MouseButtonState.Pressed)
			{
				mouseDragEndPoint = e.GetPosition(this);

				if (Math.Abs(mouseDragEndPoint.X - mouseDragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(mouseDragEndPoint.Y - mouseDragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					if (cutMode)
					{
						DragCutRectangle();
					}
					else
					{
						DragMainPage();
					}
				}
			}
		}

		// ========================
		// Control Events End
		// ========================

		// ========================
		// Drag / Cut Start
		// ========================

		// Also Refreshes Styles
		private void SetCutMode(bool mode)
		{
			cutMode = mode;
			cutCompleteButton.IsEnabled = mode;
			RefreshControlStyles();
		}

		private void DragMainPage()
		{
			double deltaX = mouseDragEndPoint.X - mouseDragStartPoint.X + mouseDragAnchorPoint.X;
			double deltaY = mouseDragEndPoint.Y - mouseDragStartPoint.Y + mouseDragAnchorPoint.Y;

			// Move the image by the difference
			Canvas.SetLeft(mainPaper, deltaX);
			Canvas.SetTop(mainPaper, deltaY);
		}

		private void DragCutRectangle()
		{
			double deltaX = mouseDragEndPoint.X - mouseDragStartPoint.X;
			double deltaY = mouseDragEndPoint.Y - mouseDragStartPoint.Y;

			// Resize the rectangle by the difference
			dragRectangle.Width = Math.Abs(deltaX);
			dragRectangle.Height = Math.Abs(deltaY);

			// Set the position of the rectangle
			Canvas.SetLeft(dragRectangle, Math.Min(mouseDragStartPoint.X, mouseDragEndPoint.X));
			Canvas.SetTop(dragRectangle, Math.Min(mouseDragStartPoint.Y, mouseDragEndPoint.Y));
		}

		// ========================
		// Drag / Cut End
		// ========================

		// Refresh the styles of the controls (button colours depending on states etc.)
		private void RefreshControlStyles()
		{
			cutModeButton.Foreground = cutMode ? cutModeOnBrush : cutModeOffBrush;
			cutCompleteButton.Foreground = cutMode ? cutCompleteOnBrush : cutCompleteOffBrush;
			previousPageButton.Foreground = CurrentPage > 0 ? cutCompleteOnBrush : cutCompleteOffBrush;
			nextPageButton.Foreground = CurrentPage < TotalPages - 1 ? cutCompleteOnBrush : cutCompleteOffBrush;

			dragRectangle.Fill = cutMode ? dragRectFillBrush : Brushes.Transparent;
			dragRectangle.Stroke = cutMode ? dragRectStrokeBrush : Brushes.Transparent;
			dragRectangle.Visibility = cutMode ? Visibility.Visible : Visibility.Hidden;
			dragRectangle.Width = 0;
			dragRectangle.Height = 0;
			//dragRectangle.Position = new Point(0, 0);
		}

		// Centre the main paper on the canvas
		private void CentreMainPaper()
		{
			var targetRes = Resolution.PaperSize((int)(mainCanvas.ActualHeight - defaultPageMargin * 3), (int)(mainCanvas.ActualWidth - defaultPageMargin * 2));

			// Set the paper size
			mainPaper.Stretch = Stretch.Fill;

			mainPaper.Height = targetRes.Height;
			mainPaper.Width = targetRes.Width;

			// Centre the paper
			Canvas.SetLeft(mainPaper, (mainCanvas.ActualWidth - mainPaper.ActualWidth) / 2);
			Canvas.SetTop(mainPaper, (mainCanvas.ActualHeight - mainPaper.ActualHeight) / 2);
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

			TotalPages = pdfDocument.PageCount;

			ShowPage(0);
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
				FileUtilities.ExportPage(page, pdfDocument, resolution, $"{exportFolderDir}{pdfHash}");
			}
		}

		public void ExportSinglePageFromPDF(int page)
		{
			if (pdfDocument == null)
			{
				throw new System.Exception("No PDF document loaded.");
			}

			Resolution resolution = Resolution.PaperSize(1100, 1100, basePaperKind);

			// Just in case the folder doesn't exist
			Directory.CreateDirectory(exportFolderDir + pdfHash);

			FileUtilities.ExportPage(page, pdfDocument, resolution, $"{exportFolderDir}{pdfHash}");
		}

		public void SetPageNumberButtonLabel(int page)
		{
			// If the page number is out of bounds, we have to add "&#xa;" character
			// in the middle to make it display correctly.
			// This is a workaround for the button not being able to display multiple lines.

			if (page > 999 || TotalPages > 999)
			{
				pageNumberButton.Content = $"{page + 1}&#xa;----&#xa;{TotalPages}";
				return;
			}

			pageNumberButton.Content = $"{page + 1}/{TotalPages}";
		}

		public void ShowPage(int page)
		{
			var bitmap = FileUtilities.LoadImage($"{exportFolderDir}{pdfHash}/{page}", Settings.Instance.ExportFormat);

			// Not while, we don't need to try a third time.
			if (bitmap == null)
			{
				ExportSinglePageFromPDF(page);
				bitmap = FileUtilities.LoadImage($"{exportFolderDir}{pdfHash}/{page}", Settings.Instance.ExportFormat);

				if (bitmap == null)
				{
					throw new System.Exception("Failed to load image.");
					// return;
				}
			}

			mainPaper.Source = FileUtilities.ConvertBitmapToImageSource(bitmap);

			SetPageNumberButtonLabel(page);
			CurrentPage = page;
			SetCutMode(false);

			if (isCanvasInitialised)
			{
				CentreMainPaper();
			}
		}
	}
}
