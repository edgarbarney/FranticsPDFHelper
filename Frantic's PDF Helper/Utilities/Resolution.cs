using System.Drawing;

namespace Frantics_PDF_Helper.Utilities
{
	public enum PaperType
	{
		// ISO
		A4,
		ISO = A4,
		// US Letter
		Letter,
		Legal,
		Tabloid,
	}

	// A more generic resolution struct instead of Point
	public struct Resolution(int width, int height)
	{
		private int width = width;
		private int height = height;

		// A lookup table for paper sizes
		private static readonly Dictionary<PaperType, Resolution> PaperResolutions = new()
		{
			{ PaperType.A4, new(210, 297) },
			{ PaperType.Letter, new(216, 279) },
			{ PaperType.Legal, new(216, 356) },
			{ PaperType.Tabloid, new(279, 432) }
		};

		public int Width
		{
			readonly get { return width; }
			set { width = value; }
		}

		public int Height
		{
			readonly get { return height; }
			set { height = value; }
		}

		public readonly override bool Equals(object? obj)
		{
			if (obj != null && obj is Resolution res)
			{
				Resolution other = res;
				return other.width == this.width && other.height == this.height;
			}
			return false;
		}

		public readonly override int GetHashCode() => HashCode.Combine(width, height);

		public readonly override string ToString() => $"{width}x{height}";

		public static bool operator ==(Resolution a, Resolution b) => a.Equals(b);

		public static bool operator !=(Resolution a, Resolution b) => !a.Equals(b);

		public static Resolution operator +(Resolution a, Resolution b) => new(a.width + b.width, a.height + b.height);

		public static Resolution operator -(Resolution a, Resolution b) => new(a.width - b.width, a.height - b.height);

		public static Resolution operator *(Resolution a, Resolution b) => new(a.width * b.width, a.height * b.height);

		public static Resolution operator /(Resolution a, Resolution b) => new(a.width / b.width, a.height / b.height);

		public static Resolution operator %(Resolution a, Resolution b) => new(a.width % b.width, a.height % b.height);

		public static Resolution operator +(Resolution a, int b) => new(a.width + b, a.height + b);

		public static Resolution operator -(Resolution a, int b) => new(a.width - b, a.height - b);

		public static Resolution operator *(Resolution a, int b) => new(a.width * b, a.height * b);

		public static Resolution operator /(Resolution a, int b) => new(a.width / b, a.height / b);

		public static Resolution operator %(Resolution a, int b) => new(a.width % b, a.height % b);

		public static Resolution PaperSize(PaperType paper = PaperType.A4) => PaperResolutions[paper];

		public static Resolution PaperSize(int scale, PaperType paper = PaperType.A4)
		{ 
			Resolution size = PaperSize(paper);
			return new(size.width * scale, size.height * scale);
		}

		public static Resolution PaperSize(int targetWidth, int targetHeight, PaperType paper = PaperType.A4)
		{
			// We don't use the nerarest multiple of the base resolution anymore
			// Instead we just use the aspect ratio of the base resolution instead.

			/*
			Resolution baseRes = PaperSize(paper);

			// Calculate scaling factors (prefer larger multiple)
			int scaleX = (int)Math.Ceiling((double)targetWidth / baseRes.width);
			int scaleY = (int)Math.Ceiling((double)targetHeight / baseRes.height);

			double scalingFactor = Math.Ceiling((double)Math.Max(scaleX, scaleY));

			// Get the nearest multiple of the base resolution
			return new((int)(scalingFactor * baseRes.width), (int)(scalingFactor * baseRes.height));
			*/

			Resolution baseRes = PaperSize(paper);

			// Calculate the aspect ratio of the base resolution
			double aspectRatio = (double)baseRes.width / baseRes.height;

			// Scale to fit within the target dimensions while preserving the aspect ratio
			double scaleWidth = targetWidth;
			double scaleHeight = scaleWidth / aspectRatio;

			if (scaleHeight > targetHeight)
			{
				scaleHeight = targetHeight;
				scaleWidth = scaleHeight * aspectRatio;
			}

			// Round down to the nearest integers
			return new((int)Math.Floor(scaleWidth), (int)Math.Floor(scaleHeight));
		}
	}
}
