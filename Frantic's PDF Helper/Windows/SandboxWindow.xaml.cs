using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

using Newtonsoft.Json;

namespace Frantics_PDF_Helper.Windows
{
	/// <summary>
	/// Interaction logic for SandboxWindow.xaml
	/// </summary>
	public partial class SandboxWindow : Window
	{
		public class DrawingAction(DrawingAction.ActionType action, object data)
		{
			public enum ActionType
			{
				Draw,
				Erase
			}

			public ActionType Action { get; set; } = action;
			public object Data { get; set; } = data;
		}

		public enum DrawMode
		{
			Freehand,
			Line,
			Rectangle,
			Ellipse,
			Erase
		}

		private readonly Image? mainImage = null;

		private Point drawBrushStartPoint;
		private Point drawBrushCurrentPoint;

		private Brush drawBrush = Brushes.Black;
		private Brush fillBrush = Brushes.Transparent;
		private double drawBrushThickness = 2.0;
		private DrawMode drawMode = DrawMode.Freehand;

		private bool isDrawing = false;
		private Shape? currentDrawnShape = null;
		private double currentDrawnShapeWidth = 0;
		private double currentDrawnShapeHeight = 0;
		private bool shouldFillShape = false;

		private List<DrawingAction> actionsHistory = [];
		private int currentActionIndex = 0;

		public SandboxWindow(ImageSource ? src = null)
		{
			InitializeComponent();
			this.Title = Localisation.GetLocalisedString("_AppName");

			if (src != null)
			{
				mainImage = new(){Source = src};
				drawCanvas.Children.Add(mainImage);
			}
		}

		public void Save()
		{
			//Let's convert the canvas children to a list of shapes
			//Then we can serialize it to JSON

			List<Shape> shapes = [];
			foreach (var child in drawCanvas.Children)
			{
				if (child is Shape shape)
				{
					shapes.Add(shape);
				}
			}

			string dataToSave = JsonConvert.SerializeObject(shapes);
		}

		public void Load()
		{
			// Load an image to the canvas
			// This is just a placeholder
			// We'll need to implement this
		}

		private void DrawCanvasClear()
		{
			// We'll just keep the image
			ClearActionHistory();
			drawCanvas.Children.Clear();
			if (mainImage != null)
			{
				drawCanvas.Children.Add(mainImage);
			}
		}

		private void DrawCanvasUndo()
		{
			var action = actionsHistory[currentActionIndex - 1];
			if (action.Action == DrawingAction.ActionType.Erase)
			{
				drawCanvas.Children.Add((Shape)action.Data);
				currentActionIndex--;
				return;
			}
			else// if (action.Action == DrawingAction.ActionType.Draw)
			{
				drawCanvas.Children.Remove((Shape)action.Data);
				currentActionIndex--;
				return;
			}
		}

		private void DrawCanvasRedo()
		{
			var action = actionsHistory[currentActionIndex];
			if (action.Action == DrawingAction.ActionType.Erase)
			{
				drawCanvas.Children.Remove((Shape)action.Data);
				currentActionIndex++;
				return;
			}
			else// if (action.Action == DrawingAction.ActionType.Draw)
			{
				drawCanvas.Children.Add((Shape)action.Data);
				currentActionIndex++;
				return;
			}
		}

		private void DrawCanvasPushCurrentShape(Shape? shape, DrawMode mode)
		{
			if (shape == null)
			{
				return;
			}

			if (mode == DrawMode.Erase)
			{
				drawCanvas.Children.Remove(shape);
				actionsHistory.Add(new (DrawingAction.ActionType.Erase, shape));
			}
			else
			{
				drawCanvas.Children.Add(shape);
				actionsHistory.Add(new (DrawingAction.ActionType.Draw, shape));
			}
			currentActionIndex++;
		}

		private void ClearActionHistory()
		{
			actionsHistory.Clear();
			currentActionIndex = 0;
		}

		private void ClearRedoHistory()
		{
			actionsHistory.RemoveRange(currentActionIndex, actionsHistory.Count - currentActionIndex);
		}

		private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				drawBrushStartPoint = e.GetPosition(this);
				drawBrushCurrentPoint = drawBrushStartPoint;

				drawBrush = new SolidColorBrush(MainColourPicker.SelectedColor);
				fillBrush = new SolidColorBrush(MainColourPicker.SecondaryColor);

				// We broke the chain!
				ClearRedoHistory();

				switch (drawMode)
				{
					case DrawMode.Freehand:
						currentDrawnShape = new Polyline();
						((Polyline)currentDrawnShape).Stroke = drawBrush;
						((Polyline)currentDrawnShape).StrokeThickness = drawBrushThickness;
						((Polyline)currentDrawnShape).Points.Add(drawBrushCurrentPoint);
						DrawCanvasPushCurrentShape(currentDrawnShape, drawMode);
						break;
					case DrawMode.Line:
						currentDrawnShape = new Line();
						((Line)currentDrawnShape).Stroke = drawBrush;
						((Line)currentDrawnShape).StrokeThickness = drawBrushThickness;
						((Line)currentDrawnShape).X1 = drawBrushCurrentPoint.X;
						((Line)currentDrawnShape).Y1 = drawBrushCurrentPoint.Y;
						((Line)currentDrawnShape).X2 = drawBrushCurrentPoint.X;
						((Line)currentDrawnShape).Y2 = drawBrushCurrentPoint.Y;
						DrawCanvasPushCurrentShape(currentDrawnShape, drawMode);
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
						DrawCanvasPushCurrentShape(currentDrawnShape, drawMode);
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
						DrawCanvasPushCurrentShape(currentDrawnShape, drawMode);
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
									DrawCanvasPushCurrentShape(shape, drawMode);
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

		private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DrawCanvasUndo();
		}

		private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DrawCanvasRedo();
		}

		private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (actionsHistory.Count > 0 && currentActionIndex > 0)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (actionsHistory.Count > 0 && currentActionIndex < actionsHistory.Count)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
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

		private void ClearAllButton_Click(object sender, RoutedEventArgs e)
		{
			if (DialogueWindow.ShowDialogue(Title, Localisation.GetLocalisedString("Generic.IrreversibleActionQuestion")))
			{
				DrawCanvasClear();
			}
		}

		private void ColourPresetButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				// Check if the button has an image as content
				// If it does, it's the transparency button
				/*
				if (button.Content is Image)
				{
					shouldFillShape = !shouldFillShape;
					MainColourPicker.SelectedColor = solidColorBrush.Color;
					return;
				}
				*/

				if (button.Background is SolidColorBrush solidColorBrush)
				{
					MainColourPicker.SelectedColor = solidColorBrush.Color;
				}
			}
		}
	}
}
