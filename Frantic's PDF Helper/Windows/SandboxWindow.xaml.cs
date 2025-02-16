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
		public enum DrawMode
		{
			Freehand,
			Line,
			Rectangle,
			Ellipse,
			Erase
		}

		private Point drawBrushStartPoint;
		private Point drawBrushCurrentPoint;

		private Brush drawBrush = Brushes.Black;
		private Brush fillBrush = Brushes.Transparent;
		private double drawBrushThickness = 2.0;
		private DrawMode drawMode = DrawMode.Rectangle;

		private bool isDrawing = false;
		private Shape? currentDrawnShape = null;
		private double currentDrawnShapeWidth = 0;
		private double currentDrawnShapeHeight = 0;
		private bool shouldFillShape = false;

		public SandboxWindow(ImageSource ? src = null)
		{
			InitializeComponent();
			this.Title = Localisation.GetLocalisedString("_AppName");

			if (src != null)
			{
				Image img = new Image();
				img.Source = src;
				drawCanvas.Children.Add(img);
			}

		}

		private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				drawBrushStartPoint = e.GetPosition(this);
				drawBrushCurrentPoint = drawBrushStartPoint;
				
				switch (drawMode)
				{
					case DrawMode.Freehand:
						currentDrawnShape = new Polyline();
						((Polyline)currentDrawnShape).Stroke = drawBrush;
						((Polyline)currentDrawnShape).StrokeThickness = drawBrushThickness;
						((Polyline)currentDrawnShape).Points.Add(drawBrushCurrentPoint);
						drawCanvas.Children.Add(currentDrawnShape);
						break;
					case DrawMode.Line:
						currentDrawnShape = new Line();
						((Line)currentDrawnShape).Stroke = drawBrush;
						((Line)currentDrawnShape).StrokeThickness = drawBrushThickness;
						((Line)currentDrawnShape).X1 = drawBrushCurrentPoint.X;
						((Line)currentDrawnShape).Y1 = drawBrushCurrentPoint.Y;
						((Line)currentDrawnShape).X2 = drawBrushCurrentPoint.X;
						((Line)currentDrawnShape).Y2 = drawBrushCurrentPoint.Y;
						drawCanvas.Children.Add(currentDrawnShape);
						break;
					case DrawMode.Rectangle:
						if (shouldFillShape)
						{
							currentDrawnShape = new Rectangle();
							((Rectangle)currentDrawnShape).Stroke = drawBrush;
							((Rectangle)currentDrawnShape).StrokeThickness = drawBrushThickness;
							((Rectangle)currentDrawnShape).Fill = fillBrush;
							((Rectangle)currentDrawnShape).Width = 0;
							((Rectangle)currentDrawnShape).Height = 0;
							Canvas.SetLeft(currentDrawnShape, drawBrushCurrentPoint.X);
							Canvas.SetTop(currentDrawnShape, drawBrushCurrentPoint.Y);
						}
						else
						{
							// To make ONLY the border hit testable, we need to create a rectangle with a border
							// We can do that with a Polyline
							currentDrawnShape = new Polyline();
							((Polyline)currentDrawnShape).Stroke = drawBrush;
							((Polyline)currentDrawnShape).StrokeThickness = drawBrushThickness;

							// To save performance, we should add the points first
							// Then modify them acoordingly later, instead of clearing and adding new points.
							for (int i = 0; i < 5; i++) // Four corners, and an extra to close the shape
							{
								((Polyline)currentDrawnShape).Points.Add(new Point());
							}
						}
						drawCanvas.Children.Add(currentDrawnShape);
						break;
					case DrawMode.Ellipse:
						currentDrawnShape = new Ellipse();
						((Ellipse)currentDrawnShape).Stroke = drawBrush;
						((Ellipse)currentDrawnShape).StrokeThickness = drawBrushThickness;
						((Ellipse)currentDrawnShape).Fill = Brushes.Transparent;
						((Ellipse)currentDrawnShape).Width = 0;
						((Ellipse)currentDrawnShape).Height = 0;
						Canvas.SetLeft(currentDrawnShape, drawBrushCurrentPoint.X);
						Canvas.SetTop(currentDrawnShape, drawBrushCurrentPoint.Y);
						drawCanvas.Children.Add(currentDrawnShape);
						break;
					case DrawMode.Erase:
						currentDrawnShape = new Line(); // Just to pass the null check
						break;
				}

				isDrawing = true;
			}
		}

		private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && isDrawing && currentDrawnShape != null)
			{
				drawBrushCurrentPoint = e.GetPosition(this);

				switch (drawMode)
				{
					case DrawMode.Freehand:
						((Polyline)currentDrawnShape).Points.Add(drawBrushCurrentPoint);
						break;
					case DrawMode.Line:
						((Line)currentDrawnShape).X2 = drawBrushCurrentPoint.X;
						((Line)currentDrawnShape).Y2 = drawBrushCurrentPoint.Y;
						break;
					case DrawMode.Rectangle:
						if (shouldFillShape)
						{
							//((Rectangle)currentDrawnShape).Fill = fillBrush;
							currentDrawnShapeWidth = Math.Abs(drawBrushCurrentPoint.X - drawBrushStartPoint.X);
							currentDrawnShapeHeight = Math.Abs(drawBrushCurrentPoint.Y - drawBrushStartPoint.Y);
							Canvas.SetLeft(currentDrawnShape, Math.Min(drawBrushStartPoint.X, drawBrushCurrentPoint.X));
							Canvas.SetTop(currentDrawnShape, Math.Min(drawBrushStartPoint.Y, drawBrushCurrentPoint.Y));
							((Rectangle)currentDrawnShape).Width = currentDrawnShapeWidth;
							((Rectangle)currentDrawnShape).Height = currentDrawnShapeHeight;
						}
						else
						{
							// Let's set the points of the polyline
							((Polyline)currentDrawnShape).Points[0] = new Point(drawBrushStartPoint.X, drawBrushStartPoint.Y);
							((Polyline)currentDrawnShape).Points[1] = new Point(drawBrushCurrentPoint.X, drawBrushStartPoint.Y);
							((Polyline)currentDrawnShape).Points[2] = new Point(drawBrushCurrentPoint.X, drawBrushCurrentPoint.Y);
							((Polyline)currentDrawnShape).Points[3] = new Point(drawBrushStartPoint.X, drawBrushCurrentPoint.Y);
							((Polyline)currentDrawnShape).Points[4] = new Point(drawBrushStartPoint.X, drawBrushStartPoint.Y);
						}
						break;
					case DrawMode.Ellipse:
						currentDrawnShapeWidth = Math.Abs(drawBrushCurrentPoint.X - drawBrushStartPoint.X);
						currentDrawnShapeHeight = Math.Abs(drawBrushCurrentPoint.Y - drawBrushStartPoint.Y);
						Canvas.SetLeft(currentDrawnShape, Math.Min(drawBrushStartPoint.X, drawBrushCurrentPoint.X));
						Canvas.SetTop(currentDrawnShape, Math.Min(drawBrushStartPoint.Y, drawBrushCurrentPoint.Y));
						((Ellipse)currentDrawnShape).Width = currentDrawnShapeWidth;
						((Ellipse)currentDrawnShape).Height = currentDrawnShapeHeight;
						break;
					case DrawMode.Erase:
						foreach (var child in drawCanvas.Children)
						{
							// We can filter for specific shapes if needed
							// Like Polylines, Rectangles, Ellipses, etc.
							if (child is Shape shape)
							{
								if (shape.IsMouseOver)
								{
									drawCanvas.Children.Remove(shape);
									break;
								}
							}
						}
						break;
				}
			}
		}

		private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Released)
			{
				isDrawing = false;
				currentDrawnShape = null;
			}
		}

		private void EraserButton_Click(object sender, RoutedEventArgs e)
		{
			drawMode = DrawMode.Erase;
		}

		private void FreehandButton_Click(object sender, RoutedEventArgs e)
		{
			drawMode = DrawMode.Freehand;
		}

		private void LineButton_Click(object sender, RoutedEventArgs e)
		{
			drawMode = DrawMode.Line;
		}

		private void RectangleButton_Click(object sender, RoutedEventArgs e)
		{
			drawMode = DrawMode.Rectangle;
		}

		private void EllipseButton_Click(object sender, RoutedEventArgs e)
		{
			drawMode = DrawMode.Ellipse;
		}
	}
}
