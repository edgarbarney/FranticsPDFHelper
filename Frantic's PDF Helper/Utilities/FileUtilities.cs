using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

using SkiaSharp;

namespace Frantics_PDF_Helper.Utilities
{
	static class FileUtilities
	{
		/// <summary>
		/// Gets the MD5 hash of a file.
		/// </summary>
		public static string GetMD5Hash(string filename)
		{
			var md5 = MD5.Create();
			var stream = File.OpenRead(filename);
			return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
		}

		/// <summary>
		/// Converts an SKBitmap to a System.Drawing.Bitmap.
		/// </summary>
		/// <param name="skBitmap">Source SKBitmap.</param>
		public static System.Drawing.Bitmap ConvertSKBitmapToDrawingBitmap(SKBitmap skBitmap)
		{
			int width = skBitmap.Width;
			int height = skBitmap.Height;

			// Step 1: Create a System.Drawing.Bitmap with the same size
			var drawingBitmap = new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);

			// Step 2: Lock the bitmap bits for writing
			BitmapData data = drawingBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			// Step 3: Get the raw pixel data from the SKBitmap
			IntPtr skBitmapData = skBitmap.GetPixels();

			// Step 4: Create a byte array to hold the pixel data
			byte[] pixels = new byte[width * height * 4]; // 4 bytes per pixel (ARGB)

			// Step 5: Copy the pixel data from the SKBitmap's IntPtr to the byte array
			Marshal.Copy(skBitmapData, pixels, 0, pixels.Length);

			// Step 6: Copy the pixel data from the byte array to the System.Drawing.Bitmap
			Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);

			// Step 7: Unlock the bits after copying
			drawingBitmap.UnlockBits(data);

			return drawingBitmap;
		}

		/// <summary>
		/// Export a single page of the PDF.
		/// </summary>
		/// <param name="page">Page number to export</param>
		/// <param name="pdfDoc">PDF Document</param>
		/// <param name="resolution">Resolution of the exported image</param>
		/// <param name="dir">Directory to export the image</param>
		public static void ExportPage(int page, PdfiumViewer.PdfDocument pdfDoc, Resolution resolution, string dir)
		{
			ExportImage(pdfDoc.Render(page, resolution.Width, resolution.Height, 96, 96, false), $"{dir}/{page}");
		}

		/// <summary>
		/// Export an image to a file.
		/// Filename should also include directory. 
		/// </summary>
		/// <remarks>
		/// The directory in the name must be existing.
		/// Name shouldn't include the file extension, the function will automatically add it.
		/// </remarks>
		/// <param name="image">Input Image</param>
		/// <param name="name">Exported Filename (including directory)</param>
		public static void ExportImage(System.Drawing.Image image, string name)
		{
			ExportImage(image, name, Settings.Instance.ExportFormat);
		}

		/// <summary>
		/// Export an image to a file.
		/// Filename should also include directory. 
		/// </summary>
		/// <remarks>
		/// The directory in the name must be existing.
		/// Name shouldn't include the file extension, the function will automatically add it.
		/// </remarks>
		/// <param name="image">Input Image</param>
		/// <param name="name">Exported Filename (including directory)</param>
		/// <param name="imgFormat">Exported Image Format</param>
		public static void ExportImage(System.Drawing.Image image, string name, ImageFormat imgFormat)
		{
			switch (imgFormat)
			{
				case ImageFormat.Png:
					image.Save($"{name}.png", System.Drawing.Imaging.ImageFormat.Png);
					break;
				case ImageFormat.Webp:
					MemoryStream tempStream = new();
					image.Save(tempStream, System.Drawing.Imaging.ImageFormat.Bmp);
					tempStream.Seek(0, SeekOrigin.Begin); // Reset stream position
					var skBitmap = SKBitmap.Decode(tempStream);
					var webpData = skBitmap.Encode(SKEncodedImageFormat.Webp, 100);
					File.WriteAllBytes($"{name}.webp", webpData.ToArray());
					break;
			}
		}

		/// <summary>
		/// Export an image to a file from a Bitmap Source.
		/// Filename should also include directory. 
		/// </summary>
		/// <remarks>
		/// The directory in the name must be existing.
		/// Name shouldn't include the file extension, the function will automatically add it.
		/// </remarks>
		/// <param name="imageSource">Input Image.
		///		<remarks>
		///			This is ImageSource isntead of BitmapSource
		///			to make it easier to use with Image controls.
		///		</remarks>
		/// </param>
		/// <param name="name">Exported Filename (including directory)</param>
		public static void ExportImage(System.Windows.Media.ImageSource image, string name)
		{
			ExportImage(image, name, Settings.Instance.ExportFormat);
		}

		/// <summary>
		/// Export an image to a file from a Bitmap Source.
		/// Filename should also include directory. 
		/// </summary>
		/// <remarks>
		/// The directory in the name must be existing.
		/// Name shouldn't include the file extension, the function will automatically add it.
		/// </remarks>
		/// <param name="imageSource">Input Image.
		///		<remarks>
		///			This is ImageSource isntead of BitmapSource
		///			to make it easier to use with Image controls.
		///		</remarks>
		/// </param>
		/// <param name="name">Exported Filename (including directory)</param>
		/// <param name="imgFormat">Exported Image Format</param>
		public static void ExportImage(System.Windows.Media.ImageSource imageSource, string name, ImageFormat imgFormat)
		{
			if (imageSource is BitmapSource bitmapSource)
			{
				switch (imgFormat)
				{
					case ImageFormat.Png:
						using (FileStream stream = new($"{name}.png", FileMode.Create))
						{
							var encoder = new PngBitmapEncoder();
							encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
							encoder.Save(stream);
						}
						break;
					case ImageFormat.Webp:
						using (MemoryStream memoryStream = new())
						{
							var encoder = new PngBitmapEncoder(); // Use PNG as an intermediate format
							encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
							encoder.Save(memoryStream);
							memoryStream.Seek(0, SeekOrigin.Begin);
							var skBitmap = SKBitmap.Decode(memoryStream);
							var skImage = SKImage.FromBitmap(skBitmap);
							var webpData = skImage.Encode(SKEncodedImageFormat.Webp, 100);
							File.WriteAllBytes($"{name}.webp", webpData.ToArray());
						}
						break;
				}
			}
			else
			{
				throw new ArgumentException("The provided ImageSource is not a BitmapSource and cannot be saved.");
			}
		}

		/// <summary>
		/// Import an <b><i>existing</i></b> image from a file.
		/// <para>
		///		For checking first, use the <see cref="LoadImage(string)"/> function.
		/// </para>
		/// </summary>
		/// <param name="path">File path, <b><i>with</i></b> extension.</param>
		/// <param name="imgFormat">Image Format to Import</param>
		public static System.Drawing.Bitmap ImportImage(string path, ImageFormat imgFormat)
		{
			switch (imgFormat)
			{
				default:
				case ImageFormat.Png:
					return new System.Drawing.Bitmap(path);

				case ImageFormat.Webp:
					var skBitmap = SKBitmap.Decode(path);
					return ConvertSKBitmapToDrawingBitmap(skBitmap);
			}
		}

		/// <summary>
		/// Import an image from a file.
		/// <para>
		///		It checks for the file's existence.
		/// </para>
		/// </summary>
		/// <param name="path">File path, <b><i>without</i></b> extension.</param>
		/// <param name="imgFormat">Image Format to Import</param>
		public static System.Drawing.Bitmap? LoadImage(string path, ImageFormat imgFormat)
		{
			path += imgFormat switch
			{
				ImageFormat.Png => ".png",
				ImageFormat.Webp => ".webp",
				_ => ".png",
			};

			if (File.Exists(path))
			{
				return ImportImage(path, imgFormat);
			}

			return null;
		}

		/// <summary>
		/// Converts given bitmap to an ImageBrush.
		/// </summary>
		/// <param name="bitmap">Bitmap Source to Convert</param>
		public static BitmapImage ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
		{
			var memoryStream = new MemoryStream();

			// Save the Bitmap to the MemoryStream as a PNG (you can use other formats like JPG if needed)
			bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

			// Create a BitmapImage and set the stream to the MemoryStream
			var bitmapImage = new BitmapImage();
			memoryStream.Seek(0, SeekOrigin.Begin);
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memoryStream;
			bitmapImage.EndInit();

			// Return the ImageSource
			return bitmapImage;
		}
	}
}
