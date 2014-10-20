namespace Flop.Testbench
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Flop;
	using Flop.Collections;
	using Flop.Testing;

	public class OptionTests
	{
		public Option<T> Find<T> (StrictList<T> list, T value)
		{
			var res = list.FindNext (value);
			return res.IsEmpty ? new Option<T> () : new Option<T> (res.First);
		}

		[Test]
		public void TestOption ()
		{
			var list = List.Create("foo", "bar", "baz");

			var res = from foo in Find (list, "foo")
					  from baz in Find (list, "baz")
					  select Tuple.Create (foo, baz);

			Check.AreEqual (res, Tuple.Create ("foo", "baz"));

			res = from foo in Find (list, "foo")
				  from biz in Find (list, "biz")
				  select Tuple.Create (foo, biz);

			Check.IsFalse (res.HasValue);

			res = from foo in Find (list, "foo")
				  from bar in Find (list, "bar")
				  where bar.Length > 3
				  select Tuple.Create (foo, bar);

			Check.IsFalse (res.HasValue);
		}
	}
}
