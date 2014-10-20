namespace Flop.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Collections;

	internal static class DefaultArbitrary
	{
		internal static void Register ()
		{
			Arbitrary.Register (new Arbitrary<char> (
				Gen.Elements (CharCandidates ().ToArray ()), 
				ShrinkChar));

			Arbitrary.Register (new Arbitrary<int> (
				Gen.Choose (0),
				x => ShrinkInteger (x).Distinct ()));

			Arbitrary.Register (new Arbitrary<long> (
				Gen.Choose (0).ToLong (), 
				x => ShrinkInteger ((int)x).Distinct ().Cast<long> ()));

			Arbitrary.Register (new Arbitrary<float> (
				Gen.Choose (0.0).ToFloat (),
				x => ShrinkDouble (x).Cast<float> ()));

			Arbitrary.Register (new Arbitrary<double> (
				Gen.Choose (0.0),
				ShrinkDouble));

			Arbitrary.Register (new Arbitrary<string> (
				from a in Arbitrary.Gen<char> ().ArrayOf ()
				select new string (a),
				x => ShrinkEnumerable (x).Select (cs => new string (cs.ToArray ()))));

			Arbitrary.Register (typeof (Enumerable<>));
			Arbitrary.Register (typeof (Array<>));
			Arbitrary.Register (typeof (AStrictList<>));
			Arbitrary.Register (typeof (ALazyList<>));
			Arbitrary.Register (typeof (ASequence<>));
		}

		private static IEnumerable<char> CharCandidates ()
		{
			for (char c = 'A'; c <= '~'; c++)
				yield return c;
			for (char c = ' '; c < 'A'; c++)
				yield return c;
			yield return '\t';
			yield return '\n';
		}

		private static IEnumerable<char> ShrinkChar (char c)
		{
			var candidates = new char[] 
				{ 'a', 'b', 'c', 'A', 'B', 'C', '1', '2', '3', char.ToLower (c), ' ', '\n' };

			return candidates.Where (x => x.SimplerThan (c)).Distinct ();
		}

		private static bool SimplerThan (this char x, char y)
		{
			Func<Func<char, bool>, bool> simpler = fun => fun (x) && !fun (y);

			return simpler (char.IsLower) || simpler (char.IsUpper) || simpler (char.IsDigit) ||
				simpler (c => c == ' ') || simpler (char.IsWhiteSpace) || x < y;
		}

		private static IEnumerable<int> ShrinkInteger (int x)
		{
			if (x < 0) yield return -x;
			yield return 0;
			for (var i = x / 2; Math.Abs (x - i) < Math.Abs (x); i = i / 2)
				yield return x - i;
		}

		private static IEnumerable<double> ShrinkDouble (double x)
		{
			if (x < 0) yield return -x;
			yield return Math.Floor (x);
		}

		private static IEnumerable<IEnumerable<T>> ShrinkEnumerable<T> (IEnumerable<T> e)
		{
			return RemoveUntil (e).Collect (Fun.Identity).Concat (ShrinkOne (e)).Prepend (new T[0]);
		}

		private static IEnumerable<IEnumerable<IEnumerable<T>>> RemoveUntil<T> (IEnumerable<T> e)
		{
			var len = e.Count ();
			for (var k = len - 1; k > 0; k = k / 2)
				yield return RemoveK (e, k, len);
		}

		private static IEnumerable<IEnumerable<T>> RemoveK<T> (IEnumerable<T> e, int k, int len)
		{
			if (k > len) return new IEnumerable<T>[0];
			var xs1 = e.Take (k);
			var xs2 = e.Skip (k);
			return (from r in RemoveK (xs2, k, len - k)
					select xs1.Concat (r)).Append (xs2);
		}

		private static IEnumerable<IEnumerable<T>> ShrinkOne<T> (IEnumerable<T> e)
		{
			if (e.IsEmpty ()) return new IEnumerable<T>[0];
			var first = e.First ();
			var rest = e.Skip (1);
			return (from x in Arbitrary.Get<T> ().Shrink (first)
					select rest.Append(x)).Concat (
					from xs in ShrinkOne (e.Skip (1))
					select xs.Append (first));
		}


		private class Enumerable<T> : ArbitraryBase<IEnumerable<T>>
		{
			public override Gen<IEnumerable<T>> Generate 
			{
				get { return Arbitrary.Gen<T> ().EnumerableOf (); }
			}

			public override IEnumerable<IEnumerable<T>> Shrink (IEnumerable<T> value)
			{
				return ShrinkEnumerable (value);
			}
		}

		private class Array<T> : ArbitraryBase<T[]>
		{
			public override Gen<T[]> Generate
			{
				get { return Arbitrary.Gen<T> ().ArrayOf (); }
			}

			public override IEnumerable<T[]> Shrink (T[] value)
			{
				return ShrinkEnumerable (value).Select (i => i.ToArray ());
			}
		}

		private class AStrictList<T> : ArbitraryBase<StrictList<T>>
		{
			public override Gen<StrictList<T>> Generate
			{
				get 
				{
					return from e in Arbitrary.Gen<T> ().EnumerableOf ()
						   select List.FromEnumerable (e);
				}
			}

			public override IEnumerable<StrictList<T>> Shrink (StrictList<T> value)
			{
				return ShrinkEnumerable (value.ToEnumerable ()).Select (List.FromEnumerable);
			}
		}

		private class ALazyList<T> : ArbitraryBase<LazyList<T>>
		{
			public override Gen<LazyList<T>> Generate
			{
				get 
				{
					return from e in Arbitrary.Gen<T> ().EnumerableOf ()
						   select LazyList.FromEnumerable (e);
				}
			}

			public override IEnumerable<LazyList<T>> Shrink (LazyList<T> value)
			{
				return ShrinkEnumerable (value.ToEnumerable ()).Select (LazyList.FromEnumerable);
			}
		}

		private class ASequence<T> : ArbitraryBase<Sequence<T>>
		{
			public override Gen<Sequence<T>> Generate
			{
				get
				{
					return from e in Arbitrary.Gen<T> ().EnumerableOf ()
						   select Sequence.FromEnumerable (e);
				}
			}

			public override IEnumerable<Sequence<T>> Shrink (Sequence<T> value)
			{
				return ShrinkEnumerable (value.ToEnumerable ()).Select (Sequence.FromEnumerable);
			}
		}
	}
}
