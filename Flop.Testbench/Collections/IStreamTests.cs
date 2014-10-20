namespace Flop.Testbench.Collections
{
	using Flop;
	using Flop.Collections;
	using Flop.Testing;

	public class IStreamTests
	{
		public static void CheckPrepend<S, T> () where S : ISequence<T>
		{
			var test = from list in Prop.Choose<S> ()
					   from item in Prop.Choose<T> ()
					   let newList = list.AddToFront (item)
					   //orderby list.Length () == 0 ? "empty list" : 
					   //             list.Length () == 1 ? "one item" : "many items"
					   select new { newList, list, item };

			test.Label ("Length is incremented by one")
				.Check (t => t.newList.Length () == t.list.Length () + 1);
			test.Label ("First item is correct")
				.Check (t => t.newList.First.Equals (t.item));
			test.Label ("The tail is equal to original list")
				.Check (t => t.newList.Rest.IsEqualTo (t.list));
		}

		private static void CheckAppend<S, T> () where S : ISequence<T>
		{
			var test = (from list in Prop.Choose<S> ()
						from item in Prop.Choose<T> ()
						let newList = list.AddToBack (item)
						select new { newList, list, item });

			test.Label ("Last item is correct")
				.Check (t => t.newList.Last ().Equals (t.item));
			test.Label ("Length is incremented by one")
				.Check (t => t.newList.Length () == t.list.Length () + 1);
			test.Label ("Original list is a proper prefix of new list")
				.Check (t => t.list.IsProperPrefixOf (t.newList));
		}

		private static void CheckDrop<S, T> () where S : IStream<T>
		{
			var test = from list in Prop.Choose<S> ()
					   from count in Prop.ForAll (Gen.Choose (0, list.Length ()))
					   //where count <= list.Length ()
					   let newList = list.Drop (count)
					   select new { newList, list, count };

			test.Label ("Length is decremented by drop count")
				.Check (t => t.newList.Length () == t.list.Length () - t.count);
			test.Label ("Either list is empty or tail is present")
				.Check (t => t.newList.IsEmpty ||
					(t.newList.First.Equals (t.list.FindNext (t.newList.First).First) &&
					t.list.IndexOf (t.newList.First).IsBetween (0, t.count) &&
					t.newList.Last ().Equals (t.list.Last ())));
		}

		private static void CheckTake<S, T> () where S : IStream<T>
		{
			var test = from list in Prop.Choose<S> ()
					   from count in Prop.ForAll (Gen.Choose (0, list.Length ()))
					   let newList = list.Take<S, T> (count)
					   select new { newList, list, count };

			test.Label ("Length is the take count")
				.Check (t => t.newList.Length () == t.count);
			test.Label ("Result is a prefix of the original list")
				.Check (t => t.newList.IsEmpty || t.newList.IsPrefixOf (t.list));
		}

		private static void CheckToString<S, T> () where S : ISequence<T>
		{
			var test = from list in Prop.Choose<S> ()
					   let str = list.ToString ("(", ")", ", ")
					   select new { list, str };

			test.Label ("Sequence as string is correct")
				.Check (t =>
					(!t.list.IsEmpty || t.str == "()") &&
					(t.list.IsEmpty ||
						(t.str == "(" + t.list.Map (e => e.ToString ())
						.ReduceLeft1 ((res, s) => res + ", " + s) + ")")));
		}

		[Test]
		public void TestPrepend ()
		{
			CheckPrepend<StrictList<int>, int> ();
			CheckPrepend<LazyList<char>, char> ();
			CheckPrepend<Sequence<string>, string> ();
		}

		[Test]
		public void TestAppend ()
		{
			CheckAppend<StrictList<int>, int> ();
			CheckAppend<LazyList<char>, char> ();
			CheckAppend<Sequence<float>, float> ();
		}

		[Test]
		public void TestDrop ()
		{
			CheckDrop<StrictList<int>, int> ();
			CheckDrop<LazyList<char>, char> ();
			CheckDrop<Sequence<float>, float> ();
		}

		[Test]
		public void TestTake ()
		{
			CheckTake<StrictList<int>, int> ();
			CheckTake<LazyList<char>, char> ();
			CheckTake<Sequence<float>, float> ();
		}

		[Test]
		public void TestSeqToString ()
		{
			CheckToString<StrictList<int>, int> ();
			CheckToString<LazyList<char>, char> ();
			CheckToString<Sequence<float>, float> ();
		}
	}
}
