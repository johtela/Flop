namespace Flop.Testbench.Collections
{
	using System;
	using System.Collections.Generic;
	using Flop.Collections;
	using Flop.Testing;

	public class SetTests
	{
		private const int _itemCount = 65;

		private IEnumerable<int> Range (int min, int max)
		{
			for (int i = 0; i < _itemCount; i++)
			{
				yield return i;
			}
		}

		private Set<int> CreateTestSet ()
		{
			return Set<int>.Create (Range (0, _itemCount));
		}

		[Test]
		public void TestTreeStructure ()
		{
			var s = Set<string>.Empty;

			s = s.Add ("0");
			s = s.Add ("1");
			s = s.Add ("3");
			s = s.Add ("4");
			s = s.Add ("5");
			Check.IsTrue (s.Contains ("0"));
			Check.IsTrue (s.Contains ("1"));
			Check.IsFalse (s.Contains ("2"));
			Runner.VConsole.ShowVisual (s.ToVisual ());
		}

		[Test]
		public void TestBalancing ()
		{
			var s = Set<int>.Empty;

			for (int i = 0; i < 80; i++)
			{
				s = s.Add (i);
			}
			Runner.VConsole.ShowVisual (s.ToVisual ());
		}

		[Test]
		public void TestAddition ()
		{
			var s = CreateTestSet ();

			for (int i = 0; i < _itemCount; i++)
			{
				Check.IsTrue (s.Contains (i));
			}
		}

		[Test]
		public void TestRemoval ()
		{
			var s = CreateTestSet ();

			s = s.Remove (33);
			Check.IsFalse (s.Contains (33));
			Check.AreEqual (_itemCount - 1, s.Count);

			s = s.Remove (55);
			Check.IsFalse (s.Contains (55));
			Check.AreEqual (_itemCount - 2, s.Count);
		}

		[Test]
		public void TestImmutability ()
		{
			var s = CreateTestSet ();

			Check.IsFalse (s.Remove (42).Contains (42));
			Check.IsTrue (s.Contains (42));
			Check.IsFalse (s.Remove (64).Contains (64));
			Check.IsTrue (s.Contains (64));
		}

		[Test]
		public void TestEnumeration ()
		{
			var s = CreateTestSet ();
			int i = 0;

			foreach (var item in s)
			{
				Check.AreEqual (i, item);
				i++;
			}                        
		}

		[Test]
		public void TestCount ()
		{
			var s = CreateTestSet ();

			Check.AreEqual (_itemCount, s.Count);
			Runner.VConsole.ShowVisual (s.ToVisual ());
		}
		
		[Test]
		public void TestDuplicatesAreIgnored ()
		{
			var s = Set<string>.Create ("foo", "bar", "cool", "bar");
			Check.AreEqual (3, s.Count);
			
			s = s.Add ("foo");
			Check.AreEqual (3, s.Count);
			Runner.VConsole.ShowVisual (s.ToVisual ());
		}
		
		[Test]
		public void TestUnion ()
		{
			var s1 = CreateTestSet ();
			var s2 = Set<int>.Create (-1, -2, -3);
			s1 = s1 + s2;
			Check.AreEqual (_itemCount + 3, s1.Count);
			
			for (int i = -3; i < _itemCount; i++)
			{
				Check.IsTrue (s1.Contains (i));
			}
		}
		
		[Test]
		public void TestIntersection ()
		{
			var s1 = CreateTestSet ();
			var s2 = Set<int>.Create (1, 2, 3);
			s1 = s1 * s2;
			Check.AreEqual (3, s1.Count);
			
			for (int i = 1; i <= 3; i++)
			{
				Check.IsTrue (s1.Contains (i));
			}
			for (int i = 4; i < _itemCount; i++)
			{
				Check.IsFalse (s1.Contains (i));
			}
		}
		
		[Test]
		public void TestSubtract ()
		{
			var s1 = CreateTestSet ();
			var s2 = Set<int>.Create (1, 2, 3);
			s1 = s1 - s2;
			Check.AreEqual (_itemCount - 3, s1.Count);
			
			for (int i = 1; i <= 3; i++)
			{
				Check.IsFalse (s1.Contains (i));
			}
			for (int i = 4; i < _itemCount; i++)
			{
				Check.IsTrue (s1.Contains (i));
			}
		}

		[Test]
		public void TestReducibility ()
		{
			var s = CreateTestSet ();
			s.Foreach (0, Check.AreEqual);
		}
	}
}