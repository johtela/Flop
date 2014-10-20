namespace Flop.Parsing
{
	using System;
	using System.Linq;
	using System.Text;
	using Flop.Collections;

	/// <summary>
	/// Parser combinator for characters and strings.
	/// </summary>
	public static class StringParser
	{
		/// <summary>
		/// Parse a given character.
		/// </summary>
		public static Parser<char, char> Char (char x)
		{
			return Parser.Satisfy<char> (y => x == y).Label (x.ToString ());
		}

		/// <summary>
		/// Parse a number [0-9]
		/// </summary>
		public static Parser<char, char> Number ()
		{
			return Parser.Satisfy<char> (char.IsNumber).Label ("number");
		}

		/// <summary>
		/// Parse a lower case character [a-z]
		/// </summary>
		public static Parser<char, char> Lower ()
		{
			return Parser.Satisfy<char> (char.IsLower).Label ("lowercase character");
		}

		/// <summary>
		/// Parse an upper case character [A-Z]
		/// </summary>
		public static Parser<char, char> Upper ()
		{
			return Parser.Satisfy<char> (char.IsUpper).Label ("uppercase character");
		}

		/// <summary>
		/// Parse any letter.
		/// </summary>
		public static Parser<char, char> Letter ()
		{
			return Parser.Satisfy<char> (char.IsLetter).Label ("letter");
		}

		/// <summary>
		/// Parse on alphanumeric character.
		/// </summary>
		public static Parser<char, char> AlphaNumeric ()
		{
			return Parser.Satisfy<char> (char.IsLetterOrDigit).Label ("alphanumeric character");
		}

		/// <summary>
		/// Parse a word (sequence of consequtive letters)
		/// </summary>
		/// <returns></returns>
		public static Parser<string, char> Word ()
		{
			return from xs in Letter ().Many ()
				   select xs.ToString ("", "", "");
		}

		/// <summary>
		/// Parse a character that is in the set of given characters.
		/// </summary>
		public static Parser<char, char> OneOf (params char[] chars)
		{
			return Parser.Satisfy<char> (c => chars.Contains (c));
		}

		/// <summary>
		/// Parse a character that is NOT in the set of given characters.
		/// </summary>
		public static Parser<char, char> NoneOf (params char[] chars)
		{
			return Parser.Satisfy<char> (c => !chars.Contains (c));
		}

		/// <summary>
		/// Parse a given sequence of characters.
		/// </summary>
		public static Parser<IStream<char>, char> CharStream (IStream<char> str)
		{
			return str.IsEmpty ? str.ToParser<IStream<char>, char> () :
				Char (str.First).Seq (
				CharStream (str.Rest).Seq (
				str.ToParser<IStream<char>, char> ()));
		}

		/// <summary>
		/// Parse a given string.
		/// </summary>
		public static Parser<string, char> String (string str)
		{
			return (from seq in CharStream (LazyList.FromEnumerable (str))
					select str).Label (str);
		}

		/// <summary>
		/// Convert a parser that returns StrictList[char] to one that returns a string.
		/// </summary>
		public static Parser<string, char> AsString (this Parser<StrictList<char>, char> parser)
		{
			return from cs in parser
				   select cs.ToString ();
		}

		/// <summary>
		/// Convenience method that combines character parser to a string parser.
		/// </summary>
		public static Parser<string, char> ManyChars (this Parser<char, char> parser)
		{
			return parser.Many ().AsString ();
		}
			 
		/// <summary>
		/// Parse a positive integer without a leading '+' character.
		/// </summary>
		public static Parser<int, char> PositiveInteger ()
		{
			return (from x in Number ()
					select x - '0').ChainLeft1 (
					Parser.ToParser<Func<int, int, int>, char> (
						(m, n) => 10 * m + n));
		}

		/// <summary>
		/// Parse a possibly negative integer.
		/// </summary>
		public static Parser<int, char> Integer ()
		{
			return from sign in Char ('-').Optional ()
				   from number in PositiveInteger ()
				   select sign.HasValue ? -number : number;
		}

		/// <summary>
		/// Creates a parser that skips whitespace, i.e. just consumes white space 
		/// from the sequence but does not return anything.
		/// </summary>
		public static Parser<Unit, char> WhiteSpace ()
		{
			return from _ in Parser.Satisfy<char> (char.IsWhiteSpace).Many1 ()
				   select Unit.Void;
		}

		public static Parser<Unit, char> Junk ()
		{
			return from _ in WhiteSpace ().Many ()
				   select Unit.Void;
		}

		public static Parser<T, char> SkipJunk<T> (this Parser<T, char> parser)
		{
			return from _ in Junk ()
				   from v in parser
				   select v;
		}

		public static Parser<string, char> Identifier ()
		{
			return from x in Letter ()
				   from xs in AlphaNumeric ().Many ()
				   select (x | xs).ToString ();
		}

		public static Parser<T, char> Token<T> (this Parser<T, char> parser)
		{
			return from v in parser
				   from _ in Junk ()
				   select v;
		}
	}
}
