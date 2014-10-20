namespace Flop.Testbench.Collections
{
	using System;
	using System.Collections.Generic;
	using Flop.Collections;
	using Flop.Testing;

	public class MapTests
	{
		private const int _itemCount = 10000;

		private IEnumerable<Tuple<int, string>> Range (int max)
		{
			for (int i = 0; i < _itemCount; i++)
			{
				yield return new Tuple<int, string>(i, i.ToString());
			}
		}

		private Map<int, string> CreateTestMap ()
		{
			return Map<int, string>.FromPairs (Range (_itemCount));
		}

		private IDictionary<int, string> CreateTestDictionary ()
		{
			var dictionary = new SortedDictionary<int, string> ();

			for (int i = 0; i < _itemCount; i++)
			{
				dictionary.Add (i, i.ToString ());
			}
			return dictionary;
		}

		[Test]
		public void TestTreeStructure ()
		{
			var map = Map<int, string>.Empty;

			map = map.Add (0, "0");
			map = map.Add (1, "1");
			Check.IsTrue (map.Contains (0));
			Check.IsTrue (map.Contains (1));
		}

		[Test]
		public void TestAddition ()
		{
			var map = CreateTestMap ();

			for (int i = 0; i < _itemCount; i++)
			{
				Check.AreEqual (i.ToString (), map [i]);
			}
		}

		[Test]
		public void TestAdditionDictionary ()
		{
			var dictionary = CreateTestDictionary ();

			for (int i = 0; i < _itemCount; i++)
			{
				Check.AreEqual (i.ToString (), dictionary [i]);
			}
		}

		[Test]
		public void TestRemoval ()
		{
			var map = CreateTestMap ();

			map = map.Remove (33);
			Check.IsFalse (map.Contains (33));
			Check.AreEqual (_itemCount - 1, map.Count);

			map = map.Remove (77);
			Check.IsFalse (map.Contains (77));
			Check.AreEqual (_itemCount - 2, map.Count);

		}

		[Test]
		public void TestImmutability ()
		{
			var map = CreateTestMap ();

			Check.IsFalse (map.Remove (42).Contains (42));
			Check.IsTrue (map.Contains (42));
			Check.IsFalse (map.Remove (64).Contains (64));
			Check.IsTrue (map.Contains (64));
		}

		[Test]
		public void TestEnumeration ()
		{
			var map = CreateTestMap ();
			int i = 0;

			foreach (var pair in map)
			{
				Check.AreEqual (i, pair.Item1);
				Check.AreEqual (i.ToString (), pair.Item2);
				i++;
			}                        
		}

		[Test]
		public void TestEnumerationDictionary ()
		{
			var dictionary = CreateTestDictionary ();
			int i = 0;

			foreach (var pair in dictionary)
			{
				Check.AreEqual (i, pair.Key);
				Check.AreEqual (i.ToString (), pair.Value);
				i++;
			}
		}

		[Test]
		public void TestCount ()
		{
			var map = CreateTestMap ();

			Check.AreEqual (_itemCount, map.Count);
		}

		[Test]
		public void TestCountDictionary ()
		{
			var dictionary = CreateTestDictionary ();

			Check.AreEqual (_itemCount, dictionary.Count);
		}
		
		[Test]
		public void TestThrowsIfDuplicate ()
		{
			Check.Throws<ArgumentException>(() =>
				Map<string, int>.FromPairs(Tuple.Create("foo", 1), 
										   Tuple.Create("bar", 2),
										   Tuple.Create("foo", 3)));
			Check.Throws<ArgumentException>(() =>
			{
				var map = Map<string, int>.Empty;
				map = map.Add("foo", 1);
				map = map.Add("bar", 2);
				map = map.Add("foo", 3);
			});
		}

		[Test]
		public void TestReducibility ()
		{
			var m = CreateTestMap ();
			m.Foreach (0, (t, i) => Check.AreEqual (i, t.Item1));
		}
	}
}
