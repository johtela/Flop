namespace Flop.Testbench
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Flop.Collections;
	using Flop.Parsing;
	using Flop.Testing;

	public class ParserMonadTests
	{
		[Test]
		public void BindTest ()
		{
			var input = Input.FromString ("foo");
			var foo = from x in StringParser.Char ('f')
					  from y in StringParser.Char ('o')
					  from z in StringParser.Char ('o')
					  select new string (new char[] { x, y, z });

			var res = foo.TryParse (input);
			Check.AreEqual ("foo", res.Left);
		}

		[Test]
		public void ParseWordTest ()
		{
			var input = Input.FromString ("abba");
			var res = StringParser.Word ().Parse (input);
			Check.AreEqual ("abba", res);
		}

		[Test]
		public void ParseIntegerTest ()
		{
			var input = Input.FromString ("1000");
			var res = StringParser.PositiveInteger ().Parse (input);
			Check.AreEqual (1000, res);
		}
	}
}
