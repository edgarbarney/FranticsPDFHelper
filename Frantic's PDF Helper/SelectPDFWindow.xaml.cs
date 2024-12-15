using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace Frantics_PDF_Helper
{
	/// <summary>
	/// Interaction logic for SelectPDFWindow.xaml
	/// </summary>
	public partial class SelectPDFWindow : Window
	{
		Microsoft.Win32.OpenFileDialog fileDialog = new();
		MainWindow mainWindow = new();

		public bool isSafeToClose = false;

		public SelectPDFWindow()
		{
			InitializeComponent();

			this.Title = Localisation.GetLocalisedString("_AppName");
			Localisation.SetTaggedButtonContent(loadFileButton, true);
			Localisation.SetTaggedButtonContent(browseFileButton, true);
		}

		private void BrowseFileButton_Click (object sender, RoutedEventArgs e)
		{
			// Open file dialog
			fileDialog.Filter = Localisation.GetLocalisedString("OpenFileDialog.pdfFiles") + " (*.pdf)|*.pdf|" + Localisation.GetLocalisedString("OpenFileDialog.allFiles") + " (*.*)|*.*";
			fileDialog.DefaultDirectory = !Path.IsPathFullyQualified(pdfFileDirTextBox.Text) ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : Path.GetDirectoryName(pdfFileDirTextBox.Text);
			fileDialog.InitialDirectory = !Path.IsPathFullyQualified(pdfFileDirTextBox.Text) ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : Path.GetDirectoryName(pdfFileDirTextBox.Text);
			if (fileDialog.ShowDialog() == true)
			{
				pdfFileDirTextBox.Text = fileDialog.FileName;
			}
		}

		private void LoadFileButton_Click (object sender, RoutedEventArgs e)
		{
			if (!File.Exists(pdfFileDirTextBox.Text))
			{
				return;
			}

			// Load PDF file
			mainWindow.LoadPDF(pdfFileDirTextBox.Text);
		}

		private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!isSafeToClose)
			{
				Application.Current.Shutdown();
			}
		}
	}
}
