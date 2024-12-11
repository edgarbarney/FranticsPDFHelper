using System;
using System.Windows;
using System.Windows.Controls;


namespace Frantics_PDF_Helper
{
	/// <summary>
	/// Interaction logic for SelectPDFWindow.xaml
	/// </summary>
	public partial class SelectPDFWindow : Window
	{
		public SelectPDFWindow()
		{
			InitializeComponent();

			this.Title = Localisation.GetLocalisedString("_AppName");
			Localisation.SetTaggedButtonContent(selectPDFButton);
			
		}
	}
}
