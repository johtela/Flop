namespace Flop
{
	using System;

	/// <summary>
	/// Option is similar to the <see cref="Nullable{T}"/> type found in the System namespace, but has one important 
	/// difference: it can encapsulate both reference and value types while Nullable can contain only value types. 
	/// In other words, Nullable has the struct restriction on its generic type parameter. Option will work with any type. 
	/// </summary>
	/// <typeparam name="T">Type of the encapsulated value.</typeparam>
	public struct Option<T>
	{
		private readonly T _value;
			
		public readonly bool HasValue;

		/// <summary>
		/// Return the encapsulated value. Throws an exception if value is not assigned.
		/// </summary>
		public T Value
		{
			get
			{
				if (HasValue)
					return _value;
				throw new InvalidOperationException ("Option has no value.");
			}
		}

		/// <summary>
		/// Create an option type with value.
		/// </summary>
		public Option(T value)
		{
			HasValue = true;
			_value = value;
		}

		/// <summary>
		/// Convert Option automatically to its encapsulated value-
		/// </summary>
		public static implicit operator T (Option<T> option)
		{
			return option.Value;
		}
	}

	/// <summary>
	/// Implement monadic extension methods for Option{T}
	/// </summary>
	public static class Option
	{
		/// <summary>
		/// Lift a value to Option monad.
		/// </summary>
		public static Option<T> ToOption<T> (this T value)
		{
			return new Option<T> (value);
		}

		/// <summary>
		/// Lift a reference value to Option monad. Null reference is interpreted as no value.
		/// </summary>
		public static Option<T> RefToOption<T> (this T value) where T: class
		{
			return value != null ? new Option<T> (value) : new Option<T> ();
		}

		/// <summary>
		/// Monadic bind.
		/// </summary>
		public static Option<U> Bind<T, U> (this Option<T> option, Func<T, Option<U>> func)
		{
			return option.HasValue ? func (option.Value) : new Option<U> ();
		}

		/// <summary>
		/// Select extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<U> Select<T, U> (this Option<T> option, Func<T, U> select)
		{
			return Bind (option, a => select (a).ToOption ());
		}

		/// <summary>
		/// SelectMany extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<V> SelectMany<T, U, V> (this Option<T> option,
			Func<T, Option<U>> project, Func<T, U, V> select)
		{
			return Bind (option, a => project (a).Bind (b => select (a, b).ToOption ()));
		}

		/// <summary>
		/// Where extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<T> Where<T> (this Option<T> option, Func<T, bool> predicate)
		{
			return option.HasValue && predicate (option.Value) ? option : new Option<T> ();
		}
	}
}
