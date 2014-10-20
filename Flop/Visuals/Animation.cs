using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using Flop.Collections;

namespace Flop.Visuals
{
	public class Animation<T> : IEnumerable<T>
	{
		internal IEnumerable<T> _values;

		public Animation (IEnumerable<T> values)
		{
			_values = values;
		}

		#region IEnumerable<T> implementation
		
		public IEnumerator<T> GetEnumerator ()
		{
			return _values.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return _values.GetEnumerator ();
		}

		#endregion

		public static Animation<T> operator+ (Animation<T> first, Animation<T> second)
		{
			return new Animation<T> (first._values.Concat (second._values));
		}

		public static Animation<T> operator++ (Animation<T> anim)
		{
			return new Animation<T> (anim._values.Loop ());
		}

		public static Animation<T> operator~ (Animation<T> anim)
		{
			return new Animation<T> (anim._values.Reverse ());
		}

		public Animation<U> Map<U> (Func<T, U> map)
		{
			return new Animation<U> (_values.Select (map));
		}

		public Animation<V> Combine<U, V> (Animation<U> other, Func<T, U, V> combine)
		{
			return new Animation<V> (_values.Combine (other, combine));
		}
	}

	public static class Animations
	{
		private static IEnumerable<T> ConstEnum<T> (T value)
		{
			while (true)
				yield return value;
		}

		private static IEnumerable<float> CosEnum (double start, double step)
		{
			for (double i = start; i < 2 * Math.PI; i += step)
				yield return (float)Math.Cos (i);
		}

		public static Animation<T> Constant<T> (T value)
		{
			return new Animation<T> (ConstEnum (value));
		}

		public static Animation<float> Cos (double start = 0, double step = Math.PI / 30)
		{
			return new Animation<float> (CosEnum (start, step));
		}

		public static Animation<Color> AnimateRGB (Animation<float> anim)
		{
			return anim.Map (f => { var i = Convert.ToInt32 (f * 255); return Color.FromArgb (i, i, i); });
	
		}
	}

	public class VisualAnimation<T>
	{
		public readonly IEnumerator<T> Values;
		public readonly Visual Visual;
		public readonly Action<Visual, T> SetValue;

		public VisualAnimation ()
		{
			
		}
	}
}
