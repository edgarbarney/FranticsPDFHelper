using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Frantics_PDF_Helper.Utilities
{
    static class WPFControlUtilities
    {
		/// <summary>
		/// Get children of a parent control.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static List<DependencyObject> GetChildren(DependencyObject parent)
		{
			var children = new List<DependencyObject>();

			// Recursively get children
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				children.Add(child);
				// Recursively get children of this child
				children.AddRange(GetChildren(child));

				// For some controls, the children are "content" of the control
				// So we need to get the children of the content as well
				// 
				// But we don't want to add strings as children
				// Until we find a better way to handle this, we'll just use this check for "containter" controls
				// As this doesn't make allows us to not make a case for every single control like:
				//
				//	if (child is GroupBox groupBox)
				//	{
				//		children.AddRange(GetChildren((DependencyObject)groupBox.Content));
				//	}


				if (child is ContentControl contentControl && contentControl.Content != null && contentControl.Content is not string)
				{
					children.AddRange(GetChildren((DependencyObject)contentControl.Content));
				}
			}

			return children;
		}

		/// <summary>
		/// Set the brush of a button icon.
		/// Button icons are usually converted from free FontAwesome icon SVGs.
		/// So they usually have similar structure.
		/// </summary>
		/// <param name="icon"></param>
		/// <param name="newBrush"></param>
		public static void SetButtonIconBrush(DrawingImage icon, Brush newBrush)
		{
			var drawingGroup = icon.Drawing as DrawingGroup;
			if (drawingGroup?.Children[0] is GeometryDrawing geometryDrawing)
			{
				geometryDrawing.Brush = newBrush;
			}
		}
	}
}
