namespace Flop.Testbench.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Flop.Collections;
	using Flop.Testing;

	public class FingerTreeTests
	{
		const int Count = 10000;
		Sequence<int> TestSeq = Sequence.FromEnumerable (Enumerable.Range (0, Count));
		Sequence<int> OtherSeq = Sequence.FromEnumerable (Enumerable.Range (Count, Count));
		

		[Test]
		public void TestLeftView ()
		{
			var viewl = TestSeq.LeftView;

			for (int i = 0; i < Count; i++)
			{
				Check.AreEqual (i, viewl.Item1);
				viewl = viewl.Item2.LeftView;	
			}
			Check.IsNull (viewl);
		}

		[Test]
		public void TestRightView ()
		{
			var viewr = TestSeq.RightView;

			for (int i = Count - 1; i >= 0; i--)
			{
				Check.AreEqual (i, viewr.Item2);
				viewr = viewr.Item1.RightView;
			}
			Check.IsNull (viewr);
		}

		[Test]
		public void TestSequenceReduction ()
		{
			TestSeq.Foreach (0, Check.AreEqual);
		}

		[Test]
		public void TestAsArray ()
		{
			TestEnumeration (TestSeq.AsArray ());
		}

		[Test]
		public void TestEnumeration ()
		{
			TestEnumeration (TestSeq.ToEnumerable ());
		}

		private void TestEnumeration (IEnumerable<int> e)
		{
			var i = 0;
			foreach (var item in e)
				Check.AreEqual (item, i++);
			Check.AreEqual (Count, i);
		}

		[Test]
		public void TestAppend ()
		{
			TestSeq.AppendWith (StrictList<int>.Empty, OtherSeq).Foreach (0, Check.AreEqual);
		}

		[Test]
		public void TestSequenceIndexing ()
		{
			for (int i = 0; i < Count; i++)	
			{
				Check.AreEqual (i, TestSeq[i]);
			}
		}

		[Test]
		public void TestReductionFromList ()
		{
			TestEnumeration (List.FromReducible (TestSeq).ToEnumerable ());
		}

		[Test]
		public void TestSplitAt ()
		{
			var split = TestSeq.SplitAt (500);
			var newSeq = split.Item1.AppendWith (List.Create (666), split.Item3);

			newSeq.Foreach (0, (i, j) =>
			{
				if (j == 500) Check.AreEqual (666, i);
				else Check.AreEqual (i, j);
			});
		}

		[Test]
		public void TestInsertion ()
		{
			var split = TestSeq.SplitAt (500);
			var newSeq = split.Item1.AppendWith (List.Create (0, 1, 2), split.Item3);
			Check.AreEqual (Count + 2, newSeq.Length);

			newSeq.Foreach (0, (i, j) =>
			{
				if (j < 500) Check.AreEqual (i, j);
				else if (j >= 500 && j < 503) Check.AreEqual (i, j - 500);
				else Check.AreEqual (i, j - 2);
			});
		}

		[Test]
		public void TestVisualization ()
		{
			var seq = Sequence.FromEnumerable (Enumerable.Range (1, 64));

			Runner.VConsole.ShowVisual (seq.ToVisual ());
		}
	}
}
