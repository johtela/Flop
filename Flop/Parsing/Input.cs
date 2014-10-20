namespace Flop.Parsing
{
	using System;
	using System.Linq;
	using System.Text;
	using Flop.Collections;

	/// <summary>
	/// Generic interface for input streams with item type S.
	/// </summary>
	public interface IInput<S> : IStream<S>
	{
		object GetPosition ();
		new IInput<S> Rest { get; }
	}

	/// <summary>
	/// Some predifined inputs are defined here.
	/// </summary>
	public static class Input
	{
		public static void CheckNotEmpty<S> (this IInput<S> input)
		{
			if (input.IsEmpty)
				throw new ParseError ("Input is exhausted.");
		}

		/// <summary>
		/// Input for strings. Position is indicated by the index of current character.
		/// </summary>
		private struct StringInt : IInput<char>
		{
			private readonly string _str;
			private readonly int _pos;

			public StringInt (string str, int pos)
			{
				_str = str;
				_pos = pos;
			}

			public object GetPosition ()
			{
				return _pos;
			}

			public char First
			{
				get 
				{
					CheckNotEmpty (this);
					return _str[_pos]; 
				}
			}

			public IInput<char> Rest
			{
				get 
				{
					CheckNotEmpty (this);
					return new StringInt (_str, _pos + 1);
				}
			}

			IStream<char> IStream<char>.Rest { get { return Rest; } }
				 
			public bool IsEmpty
			{
				get { return _pos >= _str.Length; }
			}
		}

		/// <summary>
		/// Return an input stream for the given string.
		/// </summary>
		public static IInput<char> FromString (string str)
		{
			return new StringInt (str ?? string.Empty, 0);
		}
	}
}
