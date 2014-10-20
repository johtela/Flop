namespace Flop.Visuals
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using Collections;

	public class GraphicsContext
	{
		public readonly Graphics Graphics;
		public readonly VisualStyle Style;

		public GraphicsContext (Graphics gr, VisualStyle style)
		{
			Graphics = gr;
			Style = style;
		}

		public GraphicsContext (Graphics gr) : 
			this (gr, VisualStyle.Default) {}

		public GraphicsContext (GraphicsContext gc, VisualStyle style) : 
			this (gc.Graphics, style) {}
	}
}

