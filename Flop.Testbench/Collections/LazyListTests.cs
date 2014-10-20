namespace Flop.Testbench.Collections
{
	using System;
	using System.Linq;
	using Flop;
	using Flop.Collections;
	using Flop.Testing;

	public class LazyListTests
	{
		[Test]
		public void TestEmptyList ()
		{
			LazyList<int> list = LazyList<int>.Empty;
			Check.IsTrue (list.IsEmpty);

			list = LazyList<int>.Cons (0, () => list);
			Check.IsFalse (list.IsEmpty);
		}

		[Test]
		public void TestCreationFromEmpty ()
		{
			var list = LazyList<int>.Empty;
			list = 3 | list;
			list = 2 | list;
			list = 1 | list;

			Check.AreEqual (1, list.First);
			Check.AreEqual (2, list.Rest.First);
			Check.AreEqual (3, list.Rest.Rest.First);
			Check.IsTrue (list.Rest.Rest.Rest.IsEmpty);

			Check.AreEqual (3, list.Length ());
		}

		[Test]
		public void TestCreationFromArray ()
		{
			var list = LazyList.FromArray (new int[] { 1, 2, 3 });

			Check.AreEqual (1, list.First);
			Check.AreEqual (2, list.Rest.First);
			Check.AreEqual (3, list.Rest.Rest.First);
			Check.IsTrue (list.Rest.Rest.Rest.IsEmpty);

			Check.AreEqual (3, list.Length ());
		}

		[Test]
		public void TestFindAndEqualTo ()
		{
			var list = LazyList.FromArray (new int[] { 1, 2, 3 });

			Check.IsTrue (LazyList.Create (1, 2, 3).IsEqualTo (list.FindNext (1)));
			Check.IsTrue (LazyList.Create (2, 3).IsEqualTo (list.FindNext (2)));
			Check.IsTrue (LazyList.Create (3).IsEqualTo (list.FindNext (3)));
			Check.IsTrue (LazyList<int>.Empty.IsEqualTo (list.FindNext (4)));

			Check.IsTrue (LazyList.Create (1, 2, 3).IsEqualTo (list.FindNext (i => i > 0)));
			Check.IsTrue (LazyList.Create (3).IsEqualTo (list.FindNext (i => i > 2)));
			Check.IsTrue (LazyList<int>.Empty.IsEqualTo (list.FindNext (i => i > 3)));
		}

		[Test]
		public void TestGetNthItem ()
		{
			Check.Throws<EmptyListException> (() =>
			{
				var list = LazyList.FromArray (new int[] { 1, 2, 3 });

				Check.AreEqual (1, list.Drop (0).First);
				Check.AreEqual (2, list.Drop (1).First);
				Check.AreEqual (3, list.Drop (2).First);
				Fun.Ignore (list.Drop (3).First);
			}
			);
		}

		[Test]
		public void TestEnumeration ()
		{
			var list = LazyList.Create (1, 2, 3);
			int i = 1;

			foreach (int item in list.ToEnumerable ())
			{
				Check.AreEqual (i++, item);
			}
		}

		[Test]
		public void TestToString ()
		{
			var list = LazyList.Create (1, 2, 3, 4, 5);

			Check.AreEqual ("[1, 2, 3, 4, 5]", list.ToString ());
			Check.AreEqual ("[]", LazyList<int>.Empty.ToString ());
		}

		[Test]
		public void TestCollect ()
		{
			var list = LazyList.Create (1, 2, 3);

			var res = list.Collect (i => LazyList.Create (i + 10, i + 20, i + 30));
			Check.IsTrue (res.IsEqualTo (LazyList.Create (11, 21, 31, 12, 22, 32, 13, 23, 33)));

			var res2 = LazyList<int>.Empty.Collect (i => LazyList<int>.Cons (i, () => LazyList<int>.Empty));
			Check.IsTrue (res2.IsEmpty);
		}

		[Test]
		public void TestZipWith ()
		{
			var list1 = LazyList.Create (1, 2, 3);
			var list2 = LazyList.Create ('a', 'b', 'c');

			var zipped = list1.ZipWith (list2);
			Check.AreEqual (Tuple.Create (1, 'a'), zipped.First);
			Check.AreEqual (Tuple.Create (3, 'c'), zipped.Drop (2).First);

			var listLonger = LazyList.Create ("one", "two", "three", "four");
			var listShorter = LazyList.Create (1.0, 2.0, 3.0);

			var zipped2 = listLonger.ZipWith (listShorter);
			Check.AreEqual (listShorter.Length (), zipped2.Length ());
			Check.AreEqual (Tuple.Create ("three", 3.0), zipped2.Drop (2).First);
		}

		[Test]
		public void TestMap ()
		{
			Func<int, int> timesTwo = n => n * 2;
			Check.IsTrue (LazyList.Create (1, 2, 3).Map (timesTwo).IsEqualTo (List.Create (2, 4, 6)));
			Check.IsTrue (LazyList<int>.Empty.Map (timesTwo).IsEqualTo (LazyList<int>.Empty));
			Check.IsTrue (LazyList.Create (1).Map (timesTwo).IsEqualTo (List.Create (2)));
		}
	
		[Test]
		public void TestLinq ()
		{
			var list = LazyList.FromEnumerable (Enumerable.Range (0, 10));

			var simple = from i in list
						 select i.ToString ();

			var num = 0;
			simple.Foreach (str => Check.AreEqual (num++.ToString (), str));

			var query = from i in list
						from j in list
						where i < 5 && j < 5
						select Tuple.Create (i, j);

			for (int i = 0; i < 5; i++)
				for (int j = 0; j < 5; j++)
				{
					Check.AreEqual (Tuple.Create (i, j), query.First);
					query = query.Rest as ISequence<Tuple<int, int>>;
				}
		}
	}
}