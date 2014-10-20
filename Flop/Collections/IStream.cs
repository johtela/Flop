namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public interface IStream<T>
	{
		/// <summary>
		/// The first element of the sequence.
		/// </summary>
		T First { get; }

		/// <summary>
		/// The rest of the sequence.
		/// </summary>
		IStream<T> Rest { get; }

		/// <summary>
		/// Is the sequence empty.
		/// </summary>
		bool IsEmpty { get; }
	}

	/// <summary>
	/// Exception that is thrown if empty list is accessed.
	/// </summary>
	public class EmptyListException : Exception
	{
		public EmptyListException () : base ("The list is empty") { }
	}

	/// <summary>
	/// Extension methods for IStreams.
	/// </summary>
	public static class Strm
	{
		private static Container _container;

		static Strm ()
		{
			_container = new Container (typeof (IStreamBuilder<,>));
			Register (typeof (List.Builder<>));
			Register (typeof (LazyList.Builder<>));
			Register (typeof (Sequence.Builder<>));
		}

		/// <summary>
		/// Register a new stream builder type. The type must implement the IStreamBuilder
		/// interface.
		/// </summary>
		public static void Register (Type type)
		{
			_container.Register (type);
		}

		/// <summary>
		/// Get the builer for given type.
		/// </summary>
		/// <typeparam name="S">The stream type.</typeparam>
		/// <typeparam name="T">The item type.</typeparam>
		/// <returns></returns>
		public static IStreamBuilder<S, T> Builder<S, T> () where S : IStream<T>
		{
			return (IStreamBuilder<S, T>)_container.GetImplementation (typeof (S));
		}

		/// <summary>
		/// Generically get the empty stream of given type.
		/// </summary>
		public static S Empty<S, T> () where S : IStream<T>
		{
			return Builder<S, T> ().Empty;
		}

		/// <summary>
		/// Construct a singleton stream of given type generically.
		/// </summary>
		public static S Cons<S, T> (T first) where S : IStream<T>
		{
			var b = Builder<S, T> ();
			return b.Cons (first, b.Empty);
		}

		/// <summary>
		/// Construct a stream of given type generically.
		/// </summary>
		public static S Cons<S, T> (T first, S rest) where S : IStream<T>
		{
			return Builder<S, T> ().Cons (first, rest);
		}

		/// <summary>
		/// Create a stream generically with given items.
		/// </summary>
		public static S Create<S, T> (params T[] items) where S : IStream<T>
		{
			return Builder<S, T> ().FromEnumerable (items);
		}

		/// <summary>
		/// Construct a stream generically from an enumerable.
		/// </summary>
		public static S FromEnumerable<S, T> (IEnumerable<T> items) where S : IStream<T>
		{
			return Builder<S, T> ().FromEnumerable (items);
		}

		/// <summary>
		/// The last cell of the stream. 
		/// </summary>
		public static IStream<T> End<T> (this IStream<T> seq)
		{
			var result = seq;
			while (!(result.IsEmpty || result.Rest.IsEmpty))
				result = result.Rest;
			return result;
		}

		/// <summary>
		/// The last item in the stream.
		/// </summary>
		public static T Last<T> (this IStream<T> seq)
		{
			return seq.End ().First;
		}

		/// <summary>
		/// Count the length, i.e. the number of items, in the sequence.
		/// </summary>
		public static int Length<T> (this IStream<T> seq)
		{
			int result = 0;

			while (!seq.IsEmpty)
			{
				result++;
				seq = seq.Rest;
			}
			return result;
		}

		/// <summary>
		/// Search for an item in the seqence.
		/// </summary>
		public static IStream<T> FindNext<T> (this IStream<T> seq, T item)
		{
			while (!seq.IsEmpty && !seq.First.Equals (item))
				seq = seq.Rest;
			return seq;
		}

		/// <summary>
		/// Find an item in the list that matches a predicate.
		/// </summary>
		public static IStream<T> FindNext<T> (this IStream<T> seq, Predicate<T> predicate)
		{
			while (!seq.IsEmpty && !predicate (seq.First))
				seq = seq.Rest;
			return seq;
		}

		/// <summary>
		/// Check if the stream contains given item.
		/// </summary>
		public static bool Contains<T> (this IStream<T> seq, T item)
		{
			return !FindNext (seq, item).IsEmpty;
		}

		/// <summary>
		/// Drop n items from the sequence.
		/// </summary>
		public static IStream<T> Drop<T> (this IStream<T> seq, int n)
		{
			while (n-- > 0)
				seq = seq.Rest;
			return seq;
		}

		/// <summary>
		/// Drop items from the sequence as long as the predicate holds.
		/// </summary>
		public static IStream<T> DropWhile<T> (this IStream<T> seq, Func<T, bool> predicate)
		{
			while (!seq.IsEmpty && predicate (seq.First))
				seq = seq.Rest;
			return seq;
		}

		/// <summary>
		/// Takes n first items from the sequence.
		/// </summary>
		public static S Take<S, T> (this S seq, int n) where S : IStream<T>
		{
			return FromEnumerable<S, T> (Enumerable.Take (seq.ToEnumerable (), n));
		}

		/// <summary>
		/// Takes n first items from the sequence.
		/// </summary>
		public static S TakeWhile<S, T> (this S seq, Func<T, bool> predicate) where S : IStream<T>
		{
			return FromEnumerable<S, T> (Enumerable.TakeWhile (seq.ToEnumerable (), predicate));
		}

		/// <summary>
		/// Return the position of the specified item.
		/// </summary>
		public static int IndexOf<T> (this IStream<T> seq, T item)
		{
			var i = 0;

			while (!seq.IsEmpty)
			{
				if (seq.First.Equals (item))
					return i;
				seq = seq.Rest;
				i++;
			}
			return -1;
		}

		/// <summary>
		/// Return the item in the specified position.
		/// </summary>
		public static T ItemAt<T> (this IStream<T> seq, int index)
		{
			return seq.Drop (index).First;
		}

		private static void CompareUntilDiffersOrExhausts<T> (ref IStream<T> seq, 
			ref IStream<T> other, Func<T, T, bool> equals)
		{
			while (!seq.IsEmpty && !other.IsEmpty && equals (seq.First, other.First))
			{
				seq = seq.Rest;
				other = other.Rest;
			}
		}

		private static bool StandardEquals<T> (T item1, T item2)
		{
			return item1.Equals (item2);
		}

		/// <summary>
		/// Check if two lists are equal, that is contain the same items in
		/// the same order, and have equal lengths.
		/// </summary>
		public static bool IsEqualTo<T> (this IStream<T> seq, IStream<T> other, Func<T, T, bool> equals)
		{
			CompareUntilDiffersOrExhausts (ref seq, ref other, equals);
			return seq.IsEmpty && other.IsEmpty;
		}

		/// <summary>
		/// Check if two lists are equal, that is contain the same items in
		/// the same order, and have equal lengths.
		/// </summary>
		public static bool IsEqualTo<T> (this IStream<T> seq, IStream<T> other)
		{
			return seq.IsEqualTo (other, StandardEquals);
		}

		/// <summary>
		/// Checks if the first list (this) is a prefix of the second list;
		/// </summary>
		public static bool IsPrefixOf<T> (this IStream<T> seq, IStream<T> other)
		{
			CompareUntilDiffersOrExhausts (ref seq, ref other, StandardEquals);
			return seq.IsEmpty;
		}

		/// <summary>
		/// Checks if the first list (this) is a proper prefix of the second list;
		/// </summary>
		public static bool IsProperPrefixOf<T> (this IStream<T> seq, IStream<T> other)
		{
			CompareUntilDiffersOrExhausts (ref seq, ref other, StandardEquals);
			return seq.IsEmpty && !other.IsEmpty;
		}

		/// <summary>
		/// Returns a string representing the list. Gets open, close bracket, and separtor as
		/// an argument.
		/// </summary>
		public static string ToString<T> (this IStream<T> seq, string openBracket, string closeBracket, string separator)
		{
			StringBuilder sb = new StringBuilder (openBracket);

			while (!seq.IsEmpty)
			{
				sb.Append (seq.First);
				seq = seq.Rest;

				if (!seq.IsEmpty)
					sb.Append (separator);
			}
			sb.Append (closeBracket);
			return sb.ToString ();
		}

		/// <summary>
		/// Convert sequence to IEnumerable.
		/// </summary>
		public static IEnumerable<T> ToEnumerable<T> (this IStream<T> seq)
		{
			while (!seq.IsEmpty)
			{
				yield return seq.First;
				seq = seq.Rest;
			}
		}
	}
}
