namespace Flop.Testbench.Collections
{
	using System;
	using System.Linq;
	using Flop;
	using Flop.Collections;
	using Flop.Testing;

	public class StrictListTests
	{
		[Test]
		public void TestEmptyList ()
		{
			StrictList<int> list = StrictList<int>.Empty;
			Check.IsTrue (list.IsEmpty);

			list = StrictList<int>.Cons (0, list);
			Check.IsFalse (list.IsEmpty);
		}

		[Test]
		public void TestCreationFromEmpty ()
		{
			var list = StrictList<int>.Empty;
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
			var list = List.FromArray (new int[] { 1, 2, 3 });

			Check.AreEqual (1, list.First);
			Check.AreEqual (2, list.Rest.First);
			Check.AreEqual (3, list.Rest.Rest.First);
			Check.IsTrue (list.Rest.Rest.Rest.IsEmpty);

			Check.AreEqual (3, list.Length ());
			Runner.VConsole.ShowVisual (list.ToVisual ());
		}

		[Test]
		public void TestFindAndEqualTo ()
		{
			var list = List.FromArray (new int[] { 1, 2, 3 });

			Check.IsTrue (List.Create (1, 2, 3).IsEqualTo (list.FindNext (1)));
			Check.IsTrue (List.Create (2, 3).IsEqualTo (list.FindNext (2)));
			Check.IsTrue (List.Create (3).IsEqualTo (list.FindNext (3)));
			Check.IsTrue (StrictList<int>.Empty.IsEqualTo (list.FindNext (4)));

			Check.IsTrue (List.Create (1, 2, 3).IsEqualTo (list.FindNext (i => i > 0)));
			Check.IsTrue (List.Create (3).IsEqualTo (list.FindNext (i => i > 2)));
			Check.IsTrue (StrictList<int>.Empty.IsEqualTo (list.FindNext (i => i > 3)));
		}

		[Test]
		public void TestGetNthItem ()
		{
			Check.Throws<EmptyListException> (() =>
			{
				var list = List.FromArray (new int[] { 1, 2, 3 });

				Check.AreEqual (1, list.Drop (0).First);
				Check.AreEqual (2, list.Drop (1).First);
				Check.AreEqual (3, list.Drop (2).First);
				Fun.Ignore (list.Drop (3).First);
			});
		}

		[Test]
		public void TestEnumeration ()
		{
			var list = List.Create (1, 2, 3);
			int i = 1;

			foreach (int item in list.ToEnumerable ())
			{
				Check.AreEqual (i++, item);
			}
		}

		[Test]
		public void TestInsertBefore ()
		{
			var list = List.Create (1, 2, 3);

			Check.IsTrue (List.Create (1, 2, 2, 3).IsEqualTo (list.InsertBefore (2, 3)));
			Check.IsTrue (List.Create (1, 2, 3, 4).IsEqualTo (list.InsertBefore (4, 0)));
			Check.IsTrue (List.Create (0, 1, 2, 3).IsEqualTo (list.InsertBefore (0, 1)));
			Check.IsTrue (List.Create (1).IsEqualTo (StrictList<int>.Empty.InsertBefore (1, 0)));
		}

		[Test]
		public void TestRemove ()
		{
			var list = List.Create (1, 2, 3, 4, 5);

			Check.IsTrue (List.Create (1, 2, 4, 5).IsEqualTo (list.Remove (3)));
			Check.IsTrue (List.Create (2, 3, 4, 5).IsEqualTo (list.Remove (1)));
			Check.IsTrue (List.Create (1, 2, 3, 4).IsEqualTo (list.Remove (5)));
			Check.IsTrue (list.IsEqualTo (list.Remove (0)));
		}

		[Test]
		public void TestToString ()
		{
			var list = List.Create (1, 2, 3, 4, 5);

			Check.AreEqual ("[1, 2, 3, 4, 5]", list.ToString ());
			Check.AreEqual ("[]", StrictList<int>.Empty.ToString ());

			var tuple = Tuple.Create (1, 'a');
			Check.AreEqual ("(1, a)", tuple.ToString ());
		}

		[Test]
		public void TestCollect ()
		{
			var list = List.Create (1, 2, 3);

			var res = list.Collect (i => List.Create (i + 10, i + 20, i + 30));
			Check.IsTrue (res.IsEqualTo (List.Create (11, 21, 31, 12, 22, 32, 13, 23, 33)));

			var res2 = StrictList<int>.Empty.Collect (i => List.Create (i));
			Check.IsTrue (res2.IsEmpty);
		}

		[Test]
		public void TestZipWith ()
		{
			var list1 = List.Create (1, 2, 3);
			var list2 = List.Create ('a', 'b', 'c');
			 
			var zipped = list1.ZipWith (list2);
			Check.AreEqual (Tuple.Create (1, 'a'), zipped.First);
			Check.AreEqual (Tuple.Create (3, 'c'), zipped.Last ());

			var listLonger = List.Create ("one", "two", "three", "four");
			var listShorter = List.Create (1.0, 2.0, 3.0);

			var zipped2 = listLonger.ZipWith (listShorter);
			Check.AreEqual (listShorter.Length (), zipped2.Length ());
			Check.AreEqual (Tuple.Create ("three", 3.0), zipped2.Last ());

			var zipped3 = listLonger.ZipExtendingWith (listShorter);
			Check.AreEqual (listLonger.Length (), zipped3.Length ());
			Check.AreEqual (Tuple.Create ("four", 0.0), zipped3.Last ());
		}

		[Test]
		public void TestMap ()
		{
			Func<int, int> timesTwo = n => n * 2;
			Check.IsTrue (List.Create (1, 2, 3).Map (timesTwo).IsEqualTo (List.Create (2, 4, 6)));
			Check.IsTrue (StrictList<int>.Empty.Map (timesTwo).IsEqualTo (StrictList<int>.Empty));
			Check.IsTrue (List.Cons (1).Map (timesTwo).IsEqualTo (List.Create (2)));
		}

		[Test]
		public void TestReduceWith ()
		{
			var l1 = List.Create (1, 2, 3, 4);
			var l2 = List.Create (2, 2, 2, 2);

			var sum = l1.ReduceWith (0, l2, (a, i1, i2) => a + i1 * i2);
			Check.AreEqual (20, sum);

			l2 = List.Create (2, 3);
			sum = l1.ReduceWith (0, l2, (a, i1, i2) => a + i1 * i2);
			Check.AreEqual (8, sum);
		}

		[Test]
		public void TestLinq ()
		{
			var list = LazyList.FromEnumerable (Enumerable.Range (0, 10));

			var query = from i in list
						from j in list
						select Tuple.Create (i, j);

			for (int i = 0; i < 10; i++)
				for (int j = 0; j < 10; j++)
				{
					Check.AreEqual (Tuple.Create (i, j), query.First);
					query = query.Rest as ISequence<Tuple<int, int>>;
				}
		}
	}
}