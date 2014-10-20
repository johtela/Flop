namespace Flop.Parsing
{
	using System;
	using Flop.Collections;

	public class ParseError : Exception
	{
		public ParseError (string msg) : base (msg) {}

		public static ParseError FromReply<T, S> (Reply<T, S> reply)
		{
			return new ParseError (string.Format (
				"Parse error at {0}\nUnexpected \"{1}\"\nExpected {2}", 
				reply.Input.GetPosition().ToString(), reply.Found, 
				reply.Expected.ToString ("", "", " or ")));
		}
	}
}

