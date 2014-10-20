namespace Flop.Visuals
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	
	/// <summary>
	/// Structure that is used to layout visuals.
	/// </summary>
	public struct VBox
	{
		/// <summary>
		/// The width of the box.
		/// </summary>
		public readonly float Width;
		
		/// <summary>
		/// The height of the box.
		/// </summary>
		public readonly float Height;
		
		/// <summary>
		/// Empty box.
		/// </summary>
		public static readonly VBox Empty = new VBox (0, 0);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Flop.VBox"/> struct.
		/// </summary>
		/// <param name='width'>The width of the box.</param>
		/// <param name='height'>The height of the box.</param>
		public VBox (float width, float height)
		{
			Width = width;
			Height = height;
		}
		
		/// <summary>
		/// Add another box to horizontally to this one.
		/// </summary>
		public VBox HAdd (VBox other)
		{
			return new VBox (Width + other.Width, Height);
		}
		
		/// <summary>
		/// Subtract another box from this one horizontally.
		/// </summary>
		public VBox HSub (VBox other)
		{
			return new VBox (Width - other.Width, Height);
		}
		
		/// <summary>
		/// Returns the horizontal union with another box. This means that the
		/// width of the result is the maximum of the box widths.
		/// </summary>
		public VBox HMax (VBox other)
		{
			return new VBox (Math.Max (Width, other.Width), Height);
		}
		
		/// <summary>
		/// Returns the horizontal intersection with another box. This means that the
		/// width of the result is the minimum of the box widths.
		/// </summary>
		public VBox HMin (VBox other)
		{
			return new VBox (Math.Min (Width, other.Width), Height);
		}

		/// <summary>
		/// Add another box to vertically to this one.
		/// </summary>
		public VBox VAdd (VBox other)
		{
			return new VBox (Width, Height + other.Height);
		}
		
		/// <summary>
		/// Subtract another box from this one vertically.
		/// </summary>
		public VBox VSub (VBox other)
		{
			return new VBox (Width, Height - other.Height);
		}
		
		/// <summary>
		/// Returns the vertical union with another box. This means that the
		/// height of the result is the maximum of the box heights.
		/// </summary>
		public VBox VMax (VBox other)
		{
			return new VBox (Width, Math.Max (Height, other.Height));
		}
		
		/// <summary>
		/// Returns the vertical intersection with another box. This means that the
		/// height of the result is the minimum of the box heights.
		/// </summary>
		public VBox VMin (VBox other)
		{
			return new VBox (Width, Math.Min (Height, other.Height));
		}
		
		/// <summary>
		/// Is this an empty box. A box is empty, if either its width or height
		/// is less or equal to zero.
		/// </summary>
		public bool IsEmpty
		{
			get { return Width <= 0 || Height <= 0; }
		}
	
		/// <summary>
		/// Return System.Drawing.SizeF with same dimensions.
		/// </summary>
		public SizeF AsSizeF
		{
			get { return new SizeF (Width, Height); }
		}

		public RectangleF AsRectF (Matrix matrix)
		{
			var points = new PointF[] { new PointF (0, 0) };
			matrix.TransformPoints (points);
			return new RectangleF (points[0], AsSizeF);
		}

		public override string ToString ()
		{
			return string.Format ("({0}, {1})", Width, Height);
		}
	}
}