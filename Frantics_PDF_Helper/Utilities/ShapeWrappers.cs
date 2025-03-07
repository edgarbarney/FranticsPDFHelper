using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Frantics_PDF_Helper.Utilities.ShapeSaves
{
	// Wrappers used to store shapes in a serialisable format.
	public class ShapeSave
	{
		public Brush Stroke { get; set; } = Brushes.Black;
		public double StrokeThickness { get; set; } = 2.0;
		public double X { get; set; } = 0.0;
		public double Y { get; set; } = 0.0;

		public ShapeSave() { }

		public ShapeSave(Shape shape)
		{
			Stroke = shape.Stroke;
			StrokeThickness = shape.StrokeThickness;
			//Position = new Vector2((float)shape.Margin.Left, (float)shape.Margin.Top);
		}

		// Use this constructor if the shape is on a canvas.
		public ShapeSave(Shape shape, Canvas canvas) 
		{
			Stroke = shape.Stroke;
			StrokeThickness = shape.StrokeThickness;
			X = Canvas.GetLeft(shape);
			Y = Canvas.GetTop(shape);
		}

		// Let's return to Shape back
		public Shape ToShape()
		{
			Shape shape = new Rectangle
			{
				Stroke = Stroke,
				StrokeThickness = StrokeThickness
			};
			//shape.Margin = new System.Windows.Thickness(Position.X, Position.Y, 0, 0);
			return shape;
		}
	}

	public class RectangleSave : ShapeSave
	{
		public Brush Fill { get; set; } = Brushes.Transparent;
		public double Width { get; set; } = 100.0;
		public double Height { get; set; } = 100.0;

		public RectangleSave() { }

		public RectangleSave(Rectangle rect) : base(rect)
		{
			Fill = rect.Fill;
			Width = rect.Width;
			Height = rect.Height;
		}

		public RectangleSave(Rectangle rect, Canvas canvas) : base(rect, canvas)
		{
			Fill = rect.Fill;
			Width = rect.Width;
			Height = rect.Height;
		}

		public Rectangle ToRectangle()
		{
			return new Rectangle
			{
				Fill = Fill,
				Width = Width,
				Height = Height,
				Stroke = Stroke,
				StrokeThickness = StrokeThickness
			};
		}
	}

	public class EllipseSave : ShapeSave // RectangleSave??
	{
		public Brush Fill { get; set; } = Brushes.Transparent;
		public double Width { get; set; } = 100.0;
		public double Height { get; set; } = 100.0;

		public EllipseSave() { }

		public EllipseSave(Ellipse rect) : base(rect)
		{
			Fill = rect.Fill;
			Width = rect.Width;
			Height = rect.Height;
		}

		public EllipseSave(Ellipse rect, Canvas canvas) : base(rect, canvas)
		{
			Fill = rect.Fill;
			Width = rect.Width;
			Height = rect.Height;
		}

		/// <summary>
		/// Revert this saved data back to an Ellipse.
		/// </summary>
		/// <remarks>
		/// Position should be set manually after converting back to an Ellipse.
		/// </remarks>
		/// <returns>An Ellipse</returns>
		public Ellipse ToEllipse()
		{
			return new Ellipse
			{
				Fill = Fill,
				Width = Width,
				Height = Height,
				Stroke = Stroke,
				StrokeThickness = StrokeThickness
			};
		}
	}

	public class LineSave : ShapeSave
	{
		public double X1 { get; set; } = 0.0;
		public double Y1 { get; set; } = 0.0;
		public double X2 { get; set; } = 0.0;
		public double Y2 { get; set; } = 0.0;

		public LineSave() { }

		public LineSave(Line line) : base(line)
		{
			X1 = line.X1;
			Y1 = line.Y1;
			X2 = line.X2;
			Y2 = line.Y2;
		}

		public LineSave(Line line, Canvas canvas) : base(line, canvas)
		{
			X1 = line.X1;
			Y1 = line.Y1;
			X2 = line.X2;
			Y2 = line.Y2;
		}

		public Line ToLine()
		{
			return new Line
			{
				X1 = X1,
				Y1 = Y1,
				X2 = X2,
				Y2 = Y2,
				Stroke = Stroke,
				StrokeThickness = StrokeThickness
			};
		}
	}

	public class PolylineSave : ShapeSave
	{
		public PointCollection Points { get; set; } = [];

		public PolylineSave() { }

		public PolylineSave(Polyline polyline) : base(polyline)
		{
			Points = polyline.Points;
		}

		public PolylineSave(Polyline polyline, Canvas canvas) : base(polyline, canvas)
		{
			Points = polyline.Points;
		}

		public Polyline ToPolyline()
		{
			Polyline polyline = new()
			{
				Points = Points,
				Stroke = Stroke,
				StrokeThickness = StrokeThickness
			};

			return polyline;
		}
	}
}
