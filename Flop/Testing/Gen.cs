namespace Flop.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public delegate T Gen<T> (Random rnd, int size);

	public static class Gen
	{
		/// <summary>
		/// Monadic return lifts a value to Gen monadn.
		/// </summary>
		public static Gen<T> ToGen<T> (this T value)
		{
			return (rnd, size) => value;
		}

		/// <summary>
		/// Monadic bind, the magical wand that allows composing Gens.
		/// </summary>
		public static Gen<U> Bind<T, U> (this Gen<T> gen, Func<T, Gen<U>> func)
		{
			return (rnd, size) =>
			{
				var a = gen (rnd, size);
				return func (a) (rnd, size);
			};
		}

		/// <summary>
		/// Select extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Gen<U> Select<T, U> (this Gen<T> gen, Func<T, U> select)
		{
			return gen.Bind (a => select (a).ToGen ());
		}

		/// <summary>
		/// SelectMany extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Gen<V> SelectMany<T, U, V> (this Gen<T> gen,
			Func<T, Gen<U>> project, Func<T, U, V> select)
		{
			return gen.Bind (a => project (a).Bind (b => select (a, b).ToGen ()));
		}

		/// <summary>
		/// Where extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Gen<T> Where<T> (this Gen<T> gen, Func<T, bool> predicate)
		{
			return (rnd, size) =>
			{
				T result;
				do { result = gen (rnd, size); }
				while (!predicate (result));
				return result;
			};
		}

		/// <summary>
		/// Combine two Gen values into a tuple.
		/// </summary>
		public static Gen<Tuple<T, U>> Plus<T, U> (this Gen<T> gen1, Gen<U> gen2)
		{
			return from a in gen1
				   from b in gen2
				   select Tuple.Create (a, b);
		}

		/// <summary>
		/// Combine three Gen values into a tuple.
		/// </summary>
		public static Gen<Tuple<T, U, V>> Plus<T, U, V> (this Gen<T> gen1, Gen<U> gen2,
			Gen<V> gen3)
		{
			return from a in gen1
				   from b in gen2
				   from c in gen3
				   select Tuple.Create (a, b, c);
		}

		/// <summary>
		/// Primitive generator to choose an integer.
		/// </summary>
		public static Gen<int> Choose (int min)
		{
			return (rnd, size) => rnd.Next (min, min + size);
		}

		/// <summary>
		/// Primitive generator to choose an integer in the given range.
		/// </summary>
		public static Gen<int> Choose (int min, int max)
		{
			return (rnd, size) => rnd.Next (min, max);
		}

		/// <summary>
		/// Primitive generator to choose a double.
		/// </summary>
		public static Gen<double> Choose (double min, double max)
		{
			return (rnd, size) => (rnd.NextDouble () * (max - min)) + min;
		}

		/// <summary>
		/// Primitive generator to choose a double in the given range.
		/// </summary>
		public static Gen<double> Choose (double min)
		{
			return (rnd, size) => (rnd.NextDouble () * size) + min;
		}

		/// <summary>
		/// Randomly choose an value from an array.
		/// </summary>
		public static Gen<T> Elements<T> (T[] values)
		{
			return (rnd, size) => values[rnd.Next (values.Length)];
		}

		/// <summary>
		/// Cast the gen to its base type.
		/// </summary>
		public static Gen<U> Cast<T, U> (this Gen<T> gen) where T : U
		{
			return gen.Bind (x => ((U)x).ToGen ());
		}

		public static Gen<long> ToLong (this Gen<int> gen)
		{
			return gen.Bind (x => ((long)x).ToGen ());
		}

		public static Gen<float> ToFloat (this Gen<double> gen)
		{
			return gen.Bind (x => ((float)x).ToGen ());
		}

		/// <summary>
		/// Helper function that generates an enumerable of values.
		/// </summary>
		private static IEnumerable<T> GenerateEnumerable<T> (Gen<T> gen, Random rnd, int size)
		{
			var len = rnd.Next (size);
			for (int i = 0; i < len; i++)
				yield return gen (rnd, size);
		}

		/// <summary>
		/// Returns a list (enumeration) of generated values.
		/// </summary>
		public static Gen<IEnumerable<T>> EnumerableOf<T> (this Gen<T> gen)
		{
			return (rnd, size) => GenerateEnumerable (gen, rnd, size);
		}

		/// <summary>
		/// Returns an array of generated values.
		/// </summary>
		public static Gen<T[]> ArrayOf<T> (this Gen<T> gen)
		{
			return (rnd, size) => GenerateEnumerable (gen, rnd, size).ToArray ();
		}

		/// <summary>
		/// Randomly chooses one of given generators.
		/// </summary>
		public static Gen<T> OneOf<T> (params Gen<T>[] gens)
		{
			return Choose (0, gens.Length - 1).Bind (i => gens[i]);
		}

		/// <summary>
		/// Choose a generator randomly from a list based on frequencies.
		/// </summary>
		public static Gen<T> Frequency<T> (params Tuple<int, Gen<T>>[] freqGens)
		{
			var sum = 0;
			for (int i = 0; i < freqGens.Length; i++)
				freqGens[i] = Tuple.Create (sum += freqGens[0].Item1, freqGens[i].Item2);

			return Choose (1, sum).Bind (x => freqGens.First (fg => fg.Item1 >= x).Item2);
		}
	}
}

