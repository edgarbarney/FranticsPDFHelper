using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Frantics_PDF_Helper.Windows
{
	/// <summary>
	/// Interaction logic for SandboxWindow.xaml
	/// </summary>
	public partial class SandboxWindow : Window
	{
		private Point drawBrushPoint;

		public SandboxWindow(ImageSource ? src = null)
		{
			InitializeComponent();
			this.Title = Localisation.GetLocalisedString("_AppName");

			if (src != null)
			{
				Image img = new Image();
				img.Source = src;
				Content = img;
			}

		}

		private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
				drawBrushPoint = e.GetPosition(this);
		}

		private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				drawBrushPoint = e.GetPosition(this);
			}
		}
	}
}
