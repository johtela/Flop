namespace Flop
{
	using System;

	/// <summary>
	/// A lazy value that is evaluated just before it is used,
	/// and then cached. The evaluation function needs to be pure.
	/// </summary>
	/// <typeparam name="T">The type of the lazy value.</typeparam>
	public class Lazy<T>
	{
		private bool _hasValue;
		private T _value;
		private Func<T> _evaluate;

		public Lazy (Func<T> evaluate)
		{
			_evaluate = evaluate;
		}

		public Lazy (T value)
		{
			_value = value;
			_hasValue = true;
		}

		public static implicit operator T (Lazy<T> lazy)
		{
			if (!lazy._hasValue)
			{
				lazy._value = lazy._evaluate ();
				lazy._hasValue = true;
			}
			return lazy._value;
		}
	}

	/// <summary>
	/// Static helper class for creating lazy values.
	/// </summary>
	public static class Lazy
	{
		public static Lazy<T> Create<T> (Func<T> evaluate)
		{
			return new Lazy<T> (evaluate);
		}

		public static Lazy<T> Create<T> (T value)
		{
			return new Lazy<T> (value);
		}
	}
}
