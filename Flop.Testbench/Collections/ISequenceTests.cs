namespace Flop.Testbench.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Flop;
	using Flop.Collections;
	using Flop.Testing;

	public class ISequenceTests
	{
		public static void CheckConcat<S, T> () where S : ISequence<T>
		{
			var test = from list1 in Prop.Choose<S> ()
					   from list2 in Prop.Choose<S> ()
					   let conList = (S)list1.Concat (list2)
					   select new { list1, list2, conList };

			test.Label ("Length is sum of input list lengths")
				.Check (t => t.conList.Length () == t.list1.Length () + t.list2.Length ());
			test.Label ("The prefix is correct")
				.Check (t => t.conList.Take<S, T> (t.list1.Length ()).IsEqualTo (t.list1));
			test.Label ("The postfix is correct")
				.Check (t => t.conList.Drop (t.list1.Length ()).IsEqualTo (t.list2));
		}

		public static void CheckMap<S, T> (Func<T, T> map) where S : ISequence<T>
		{
			var test = from list in Prop.Choose<S> ()
					   let mapList = (S)list.Map (map)
					   select new { list, mapList };

			test.Label ("Result length is the same as original length")
				.Check (t => t.mapList.Length () == t.list.Length ());
			test.Label ("The items in mapped list are correct")
				.Check (t => t.list.ZipWith (t.mapList).ToEnumerable ().All (
					p => p.Item2.Equals (map (p.Item1))));
		}

		public static void CheckFilter<S, T> (Func<T, bool> predicate) where S : ISequence<T>
		{
			var test = from list in Prop.Choose<S> ()
					   let filtList = (S)list.Filter (predicate)
					   select new { list, filtList };

			test.Label ("Result length <= original length")
				.Check (t => t.filtList.Length () <= t.list.Length ());
			test.Label ("All items in the result satisfy the predicate")
				.Check (t => t.filtList.ToEnumerable ().All (e => predicate(e)));
			test.Label ("If predicate is false, item is not in result")
				.Check (t => t.list.ToEnumerable ().All (e => predicate(e) || !t.filtList.Contains (e)));
		}

		public static void CheckCollect<R, S, T> () 
			where R : ISequence<S>
			where S : ISequence<T>
		{
			var test = from lists in Prop.Choose<R> ()
					   let result = (S)lists.Collect (s => s)
					   select new { lists, result };

			test.Label ("Result length is the sum of list lengths")
				.Check (t => t.result.Length () == t.lists.ToEnumerable ().Sum (s => s.Length ()));
			test.Label ("Result is concatenation of source lists")
				.Check (t => t.lists.ReduceLeft (Strm.Empty<S, T> (), (res, s) => (S)res.Concat (s)).IsEqualTo (t.result));
		}

		public static void CheckZipWith<S, T> () where S : ISequence<T>
		{
			var test = from list1 in Prop.Choose<S> ()
					   from list2 in Prop.Choose<S> ()
					   let zipList = list1.ZipWith (list2)
					   select new { list1, list2, zipList };

			test.Label ("Result length == shorter list's length.")
				.Check (t => t.zipList.Length () == Math.Min (t.list1.Length (), t.list2.Length ()));
			test.Label ("Result items are correct")
				.Check (t => t.zipList.IterateWhile (0,
					(pair, i) => t.list1.ItemAt (i).Equals (pair.Item1) && 
								t.list2.ItemAt (i).Equals (pair.Item2)));
		}

		public static void CheckReduceLeft<S, T> () where S : ISequence<T>
		{
			(from list in Prop.Choose<S> ()
			 let revList = list.ReduceLeft (Strm.Empty<S, T> (), (s, i) => Strm.Cons<S, T> (i, s))
			 select new { list, revList })
			.Label ("Reduced list is reverse of original list")
			.Check (t => t.list.Reverse<S, T> ().IsEqualTo (t.revList));
		}

		public static void CheckReduceRight<S, T> () where S : ISequence<T>
		{
			(from list in Prop.Choose<S> ()
			 let revList = list.ReduceRight ((i, s) => Strm.Cons<S, T> (i, s), Strm.Empty<S, T> ())
			 select new { list, revList })
			.Label ("Reduced list is same as original list")
			.Check (t => t.list.IsEqualTo (t.revList));
		}

		public static void CheckReverse<S, T> () where S : ISequence<T>
		{
			var test = from list in Prop.Choose<S> ()
					   let revList = list.Reverse<S, T> ()
					   select new { list, revList, len = list.Length () };

			test.Label ("Length is same as original length")
				.Check (t => t.len == t.revList.Length ());
			test.Label ("Empty and singleton reversed == original list")
				.Check (t => (t.len > 1) || t.list.IsEqualTo (t.revList));
			test.Label ("Longer lists are reversed")
				.Check (t => t.len < 2 || Enumerable.Range (0, t.len / 2).All (
					i => t.list.ItemAt (i).Equals (t.revList.ItemAt (t.len - 1 - i))));
		}

		[Test]
		public void TestConcat ()
		{
			CheckConcat<StrictList<int>, int> ();
			CheckConcat<LazyList<float>, float> ();
			CheckConcat<StrictList<string>, string> ();
		}

		[Test]
		public void TestMap ()
		{
			CheckMap<StrictList<int>, int> (i => i * 2);
			CheckMap<LazyList<float>, float> (f => f / 3);
			CheckMap<StrictList<string>, string> (s => s.ToUpper ());
		}

		[Test]
		public void TestFilter ()
		{
			Func<int, bool> isEven = i => (i % 2) == 0;
			CheckFilter<StrictList<int>, int> (isEven);
			CheckFilter<LazyList<int>, int> (isEven);
			CheckFilter<StrictList<int>, int> (isEven);
		}

		[Test]
		public void TestCollect ()
		{
			CheckCollect<StrictList<StrictList<int>>, StrictList<int>, int> ();
			CheckCollect<LazyList<LazyList<float>>, LazyList<float>, float> ();
			CheckCollect<Sequence<Sequence<string>>, Sequence<string>, string> ();
		}

		[Test]
		public void TestZipWith ()
		{
			CheckZipWith<StrictList<int>, int> ();
			CheckZipWith<LazyList<float>, float> ();
			CheckZipWith<Sequence<string>, string> ();
		}

		[Test]
		public void TestReduceLeft ()
		{
			CheckReduceLeft<StrictList<int>, int> ();
			CheckReduceLeft<LazyList<float>, float> ();
			CheckReduceLeft<Sequence<string>, string> ();
		}

		[Test]
		public void TestReduceRight ()
		{
			CheckReduceRight<StrictList<int>, int> ();
			CheckReduceRight<LazyList<float>, float> ();
			CheckReduceRight<Sequence<string>, string> ();
		}

		[Test]
		public void TestReverse ()
		{
			CheckReverse<StrictList<int>, int> ();
			CheckReverse<LazyList<float>, float> ();
			CheckReverse<Sequence<string>, string> ();
		}
	}
}
