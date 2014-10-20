namespace Flop.Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Flop.Collections;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Text;
	
	/// <summary>
	/// Interface that can be implemented by any object that
	/// can be visualized with the elements of the Visual library.
	/// </summary>
	public interface IVisualizable
	{
		Visual ToVisual ();
	}

	/// <summary>
	/// Enumeration for defining the stack direction.
	/// </summary>
	public enum VisualDirection { Horizontal, Vertical }
	
	/// <summary>
	/// Horizontal alignment of the items in a stack.
	/// </summary>
	public enum HAlign { Left, Center, Right }
	
	/// <summary>
	/// Vertical alignment of the items in a stack.
	/// </summary>
	public enum VAlign { Top, Center, Bottom }

	/// <summary>
	/// Type of frame to be drawn.
	/// </summary>
	public enum FrameKind { Rectangle, Ellipse, RoundRectangle }
		
	/// <summary>
	/// A visual is a drawable figure that knows how to calculate
	/// its size. It is drawn in a two-pass algorithm with first
	/// phase calculating the required size, and the second phase
	/// actually drawing the graphics.
	/// 
	/// Visuals can be composed with other visuals to create complex
	/// layout structures. The simple primitives and collections of
	/// visuals suchs as stacks are used to create more complex ones.
	/// </summary>
	public abstract class Visual
	{
		private Nullable<VBox> _size;

		/// <summary>
		/// An abstract method that calculates the size of a visual once it is constructed.
		/// </summary>
		/// <param name="context">The graphics context which used in drawing.</param>
		/// <returns>The desired size of the visual.</returns>
		protected abstract VBox CalculateSize (GraphicsContext context);
		
		/// <summary>
		/// Draw the visual into specified context using the available size.
		/// </summary>
		/// <param name="context">The graphics context which used in drawing.</param>
		/// <param name='availableSize'>The available size into which the visual should
		/// fit.</param>
		protected abstract void Draw (GraphicsContext context, VBox availableSize);
		
		public void Render (GraphicsContext context, VBox availableSize)
		{
			Draw (context, availableSize);
		}

		public VBox GetSize (GraphicsContext context)
		{
			if (_size == null)
				_size = CalculateSize (context);
			return _size.Value;
		}

		/// <summary>
		/// Empty visual is handy for creating placeholders.
		/// </summary>
		private class _Empty : Visual
		{
			protected override VBox CalculateSize (GraphicsContext context)
			{
				return VBox.Empty;
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
			}
		}

		/// <summary>
		/// Helper base class to create wrapped visuals.
		/// </summary>
		private abstract class _Wrapped : Visual
		{
			public readonly Visual Visual;

			public _Wrapped (Visual visual)
			{
				Visual = visual;
			}

			protected override VBox CalculateSize (GraphicsContext context)
			{
				return Visual.CalculateSize (context);
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				Visual.Render (context, availableSize);
			}
		}

		/// <summary>
		/// A label that renders static text.
		/// </summary>
		private sealed class _Label : Visual
		{
			/// <summary>
			/// The text to be rendered.
			/// </summary>
			public readonly string Text;

			/// <summary>
			/// A constructor for the label.
			/// </summary>
			public _Label (string text)
			{
				Text = text;
			}

			/// <summary>
			/// Calculates the size of the label.
			/// </summary>
			protected override VBox CalculateSize (GraphicsContext context)
			{
				return new VBox (context.Graphics.MeasureString (Text, context.Style.Font).Width, 
					context.Style.Font.Height);
			}
			
			/// <summary>
			/// Draw the label into the specified context.
			/// </summary>
			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				context.Graphics.DrawString (Text, context.Style.Font, context.Style.TextBrush, 
					new PointF (0, 0));
			}
		}

		/// <summary>
		/// Add margins to a visual.
		/// </summary>
		private sealed class _Margin : _Wrapped
		{
			public readonly float Left;
			public readonly float Right;
			public readonly float Top;
			public readonly float Bottom;

			public _Margin (Visual visual, float left, float right, float top, float bottom)
				: base (visual)
			{
				Left = left;
				Right = right;
				Top = top;
				Bottom = bottom;
			}

			protected override VBox CalculateSize (GraphicsContext context)
			{
				var box = base.CalculateSize (context);
				return new VBox (box.Width + Left + Right, box.Height + Top + Bottom);
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				var st = context.Graphics.Save ();
				context.Graphics.TranslateTransform (Left, Top);
				base.Draw (context, availableSize);
				context.Graphics.Restore (st);
			}
		}

		/// <summary>
		/// Stack of visuals that are laid out either horizontally (left to right) or
		/// vertically (top to bottom).
		/// </summary>
		private sealed class _Stack : Visual
		{
			/// <summary>
			/// The visuals in the stack.
			/// </summary>
			public readonly ISequence<Visual> Items;
			
			/// <summary>
			/// The direction of the stack (horizontal or vertical)
			/// </summary>
			public readonly VisualDirection Direction;
			
			/// <summary>
			/// This setting controls how the items in stack are aligned horizontally,
			/// that is, whether they are aligned by their left or right edges or centered. 
			/// The setting only has effect, if the stack is vertical.
			/// </summary>
			public readonly HAlign HorizAlign;
			
			/// <summary>
			/// This setting controls how the items in stack are aligned vertically,
			/// that is, whether they are aligned by their top or bottom edges or centered. 
			/// The setting only has effect, if the stack is horizontal.
			/// </summary>
			public readonly VAlign VertAlign;
			
			/// <summary>
			/// Initializes a new stack.
			/// </summary>
			public _Stack (ISequence<Visual> items, VisualDirection direction, HAlign horizAlign,
				VAlign vertAlign)
			{
				Items = items;
				Direction = direction;
				HorizAlign = horizAlign;
				VertAlign = vertAlign;
			}
			
			/// <summary>
			/// Override to calculates the size of the visual. 
			/// </summary>
			/// <description>
			/// If the stack is horizontal, the width of the stack is the sum of the 
			/// widths of the visuals in it. The height of the stack is the height of 
			/// the tallest item.<para/>
			/// If the stack is vertical, the height of the stack is the sum of the 
			/// heights of the visuals in it. The width of the stack is the with of 
			/// the widest item.
			/// </description>
			protected override VBox CalculateSize (GraphicsContext context)
			{
				return ((IReducible<Visual>)Items).ReduceLeft (VBox.Empty, (acc, v) => 
				{
					var box = v.CalculateSize (context);
					return Direction == VisualDirection.Horizontal ?
						acc.VMax (box).HAdd (box) :
						acc.HMax (box).VAdd (box);
				}
				);
			}
			
			/// <summary>
			/// Calulate the horizontal offset of a visual based on the alignment.
			/// </summary>
			private float DeltaX (float outerWidth, float innerWidth)
			{
				switch (HorizAlign)
				{
					case HAlign.Center:
						return (outerWidth - innerWidth) / 2;
					case HAlign.Right:
						return outerWidth - innerWidth;
					default:
						return 0;
				}
			}
			
			/// <summary>
			/// Calulate the vertical offset of a visual based on the alignment.
			/// </summary>
			private float DeltaY (float outerHeight, float innerHeight)
			{
				switch (VertAlign)
				{
					case VAlign.Center:
						return (outerHeight - innerHeight) / 2;
					case VAlign.Bottom:
						return outerHeight - innerHeight;
					default:
						return 0;
				}
			}
			
			/// <summary>
			/// Draw the stack into the specified context.
			/// </summary>
			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				var stack = GetSize (context);

				Items.Foreach (visual =>
				{
					if (availableSize.IsEmpty)
						return;

					var inner = visual.GetSize (context);
					var outer = Direction == VisualDirection.Horizontal ?
						new VBox (inner.Width, stack.Height) :
						new VBox (stack.Width, inner.Height);
					var st = context.Graphics.Save ();
					context.Graphics.TranslateTransform (DeltaX (outer.Width, inner.Width),
						DeltaY (outer.Height, inner.Height));
					visual.Render (context, outer);
					context.Graphics.Restore (st);

					if (Direction == VisualDirection.Horizontal)
					{
						context.Graphics.TranslateTransform (outer.Width, 0);
						availableSize = availableSize.HSub (outer);
					}
					else
					{
						context.Graphics.TranslateTransform (0, outer.Height);
						availableSize = availableSize.VSub (outer);
					}
				});
			}
		}

		/// <summary>
		/// Hidden visual that has the same size as the undelying visual.
		/// </summary>
		private sealed class _Hidden : _Wrapped
		{
			public _Hidden (Visual visual) : base (visual) { }

			protected override void Draw (GraphicsContext context, VBox availableSize) { }
		}

		/// <summary>
		/// Horizontal of vertical ruler.
		/// </summary>
		private sealed class _Ruler : Visual
		{
			public readonly VisualDirection Direction;

			public _Ruler (VisualDirection direction)
			{
				Direction = direction;
			}

			protected override VBox CalculateSize (GraphicsContext context)
			{
				return new VBox (8, 8);
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				if (Direction == VisualDirection.Horizontal)
				{
					var y = availableSize.Height / 2;
					context.Graphics.DrawLine (context.Style.Pen, 0, y, availableSize.Width, y);
				}
				else
				{
					var x = availableSize.Width / 2;
					context.Graphics.DrawLine (context.Style.Pen, x, 0, x, availableSize.Height);
				}
			}
		}

		/// <summary>
		/// Text editor.
		/// </summary>
		private sealed class _TextEdit : Visual
		{
			private string _text;
			private Action<string> _accept;

			protected override VBox CalculateSize (GraphicsContext context)
			{
				throw new NotImplementedException ();
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Frame a visual.
		/// </summary>
		private sealed class _Frame : _Wrapped
		{
			public readonly FrameKind Kind;

			public _Frame (Visual visual, FrameKind kind) : base (visual) 
			{
				Kind = kind;
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				var box = Visual.GetSize (context);
				base.Draw (context, availableSize);

				switch (Kind)
				{
					case FrameKind.Rectangle:
						context.Graphics.DrawRectangle (context.Style.Pen, 
							0, 0, box.Width - 1, box.Height - 1);
						break;
					case FrameKind.Ellipse:
						context.Graphics.DrawEllipse (context.Style.Pen, 
							0, 0, box.Width - 1, box.Height - 1);
						break;
					case FrameKind.RoundRectangle:
						DrawRoundedRectangle(context.Graphics, context.Style.Pen,
							new RectangleF(0, 0, box.Width - 1, box.Height - 1), 10);
						break;
				}
			}
		
			private static void DrawRoundedRectangle (Graphics graphics, Pen pen,
				RectangleF rect, float radius)
			{
				var gp = new GraphicsPath ();

				gp.AddArc (rect.X, rect.Y, radius, radius, 180, 90);
				gp.AddArc (rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
				gp.AddArc (rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
				gp.AddArc (rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
				gp.AddLine (rect.X, rect.Y + rect.Height - radius, rect.X, rect.Y + radius / 2);

				graphics.DrawPath (pen, gp);
			}
		}

		/// <summary>
		/// Apply a new style to a visual.
		/// </summary>
		private sealed class _Styled : _Wrapped
		{
			public readonly VisualStyle Style;

			public _Styled (Visual visual, VisualStyle style) : base (visual) 
			{
				Style = style;
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				Visual.Render (new GraphicsContext(context, Style), availableSize);
			}
		}

		/// <summary>
		/// Target of a connector.
		/// </summary>
		private class _Anchor : _Wrapped
		{
			public PointF Position;
			public readonly HAlign HorizAlign;
			public readonly VAlign VertAlign;

			public _Anchor (Visual visual, HAlign horizAlign, VAlign vertAlign)
				: base (visual)
			{
				HorizAlign = horizAlign;
				VertAlign = vertAlign;
			}

			private PointF GetAnchorPosition (Matrix matrix, VBox box)
			{
				var anchor = new PointF[1];
				switch (HorizAlign)
				{
					case HAlign.Left:
						anchor[0].X = 0;
						break;
					case HAlign.Center:
						anchor[0].X = box.Width / 2;
						break;
					case HAlign.Right:
						anchor[0].X = box.Width;
						break;
				}
				switch (VertAlign)
				{
					case VAlign.Top:
						anchor[0].Y = 0;
						break;
					case VAlign.Center:
						anchor[0].Y = box.Height / 2;
						break;
					case VAlign.Bottom:
						anchor[0].Y = box.Height;
						break;
				}
				matrix.TransformPoints (anchor);
				return anchor[0];
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				Position = GetAnchorPosition (context.Graphics.Transform, Visual.GetSize (context));
				base.Draw (context, availableSize);
			}
		}

		/// <summary>
		/// Connector draws a line to an anchor.
		/// </summary>
		private sealed class _Connector : _Anchor
		{
			public readonly _Anchor Target;

			public _Connector (Visual visual, _Anchor target, HAlign horizAlign, VAlign vertAlign)
				: base (visual, horizAlign, vertAlign)
			{
				Target = target;
			}

			protected override void Draw (GraphicsContext context, VBox availableSize)
			{
				base.Draw (context, availableSize);
				var state = context.Graphics.Save ();
				context.Graphics.ResetTransform ();
				context.Graphics.DrawLine (context.Style.Pen, Position, Target.Position);
				context.Graphics.Restore (state);
			}
		}

		/// <summary>
		/// Create an empty visual.
		/// </summary>
		public static Visual Empty ()
		{
			return new _Empty ();
		}

		/// <summary>
		/// Create a new label.
		/// </summary>
		public static Visual Label (string text)
		{
			return new _Label (text);
		}
		
		/// <summary>
		/// Create a horizontal stack.
		/// </summary>
		public static Visual HStack (VAlign alignment, ISequence<Visual> visuals)
		{
			return new _Stack (visuals, VisualDirection.Horizontal, HAlign.Left, alignment);
		}
		
		/// <summary>
		/// Create a horizontal stack.
		/// </summary>
		public static Visual HStack (VAlign alignment, params Visual[] visuals)
		{
			return new _Stack (List.Create (visuals), VisualDirection.Horizontal, HAlign.Left, alignment);
		}
		
		/// <summary>
		/// Create a vertical stack.
		/// </summary>
		public static Visual VStack (HAlign alignment, ISequence<Visual> visuals)
		{
			return new _Stack (visuals, VisualDirection.Vertical, alignment, VAlign.Top);
		}
		
		/// <summary>
		/// Create a vertical stack.
		/// </summary>
		public static Visual VStack (HAlign alignment, params Visual[] visuals)
		{
			return new _Stack (List.Create (visuals), VisualDirection.Vertical, alignment, VAlign.Top);
		}

		/// <summary>
		/// Hide a visual.
		/// </summary>
		public static Visual Hidden (Visual visual)
		{
			return new _Hidden (visual);
		}

		/// <summary>
		/// Surrond a visual horizontally by parentheses.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static Visual Parenthesize (Visual v)
		{
			return HStack (VAlign.Top, Label ("("), v, Label (")"));
		}

		/// <summary>
		/// Create a margin with a width of n X characters.
		/// </summary>
		public static Visual Margin (Visual visual, float left = 0, float right = 0, 
			float top = 0, float bottom = 0)
		{
			return new _Margin (visual, left, right, top, bottom);
		}

		/// <summary>
		/// Create a horizontal ruler.
		/// </summary>
		public static Visual HRuler ()
		{
			return new _Ruler (VisualDirection.Horizontal);
		}

		/// <summary>
		/// Create a vertical ruler.
		/// </summary>
		public static Visual VRuler ()
		{
			return new _Ruler (VisualDirection.Vertical);
		}

		/// <summary>
		/// Frame a visual with a rectangle.
		/// </summary>
		public static Visual Frame (Visual visual, FrameKind kind)
		{
			return new _Frame (visual, kind);
		}

		/// <summary>
		/// Create an anchor around a visual to act as a target of a connector.
		/// </summary>
		public static Visual Anchor (Visual visual, HAlign horizAlign, VAlign vertAlign)
		{
			return new _Anchor (visual, horizAlign, vertAlign);
		}

		/// <summary>
		/// Draws a connector between visuals. The target visual must be wrapped by an
		/// anchor in order to draw a connector to it.
		/// </summary>
		public static Visual Connector (Visual visual, Visual target, HAlign horizAlign, VAlign vertAlign)
		{
			if (!(target is _Anchor))
				throw new ArgumentException ("Target visual must be surronded by an anchor", "target");
			return new _Connector(visual, target as _Anchor, horizAlign, vertAlign);
		}

		/// <summary>
		/// Draws first a paragraph with a header and indented body. The amount
		/// of indentation is given as the last parameter.
		/// </summary>
		public static Visual Indented (Visual header, Visual body, int indent)
		{
			return VStack (HAlign.Left, header, Margin (body, left: indent));
		}

		/// <summary>
		/// Return the visualization of any object. This works by first chechking if the object
		/// implements the IVisualizable interface; if not, a label is returned with value of the
		/// object's ToString function.
		/// </summary>
		public static Visual Visualize (object obj)
		{
			return obj is IVisualizable ?
				(obj as IVisualizable).ToVisual () :
				Label (obj.ToString ());
		}
	}
}