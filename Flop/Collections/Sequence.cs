namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Flop.Visuals;

	/// <summary>
	/// Random access sequence built on top of finger tree.
	/// </summary>
	/// <typeparam name="T">The item type of the sequence.</typeparam>
	public struct Sequence<T> : ISequence<T>, IVisualizable
	{
		/// <summary>
		/// The size monoid defines how tree sizes are accumulated.
		/// The size is just an integer and the plus operation is
		/// the normal adition.
		/// </summary>
		public struct Size : IMonoid<Size>
		{
			private int _value;

			public Size (int value)
			{
				_value = value;
			}

			public static implicit operator int (Size ts)
			{
				return ts._value;
			}

			public Size Plus (Size other)
			{
				return new Size (_value + other._value);
			}

			public override string ToString ()
			{
				return _value.ToString ();
			}
		}

		/// <summary>
		/// The wrapper structure for elemens of the sequence.
		/// This is needed to implement the IMeasurable interface.
		/// </summary>
		public struct Elem : IMeasurable<Size>
		{
			public readonly T Value;

			public Elem (T value)
			{
				Value = value;
			}

			public static implicit operator T (Elem e)
			{
				return e.Value;
			}

			public Size Measure ()
			{
				return new Size (1);
			}

			public override bool Equals (object obj)
			{
				return obj is Elem && Value.Equals (((Elem)obj).Value);
			}

			public override int GetHashCode ()
			{
				return Value.GetHashCode ();
			}

			public override string ToString ()
			{
				return Value.ToString ();
			}
		}

		// The finger tree wrapped in the Sequence.
		private FingerTree<Elem, Size> _tree;

		private Sequence (FingerTree<Elem, Size> tree)
		{
			_tree = tree;
		}

		private static Elem CreateElem (T item)
		{
			return new Elem (item);
		}

		private static Func<Size, bool> FindP (int index)
		{
			return sz => index < sz;
		}

		/// <summary>
		/// Create a sequence from enumerable.
		/// </summary>
		public static Sequence<T> FromEnumerable (IEnumerable<T> items)
		{
			return new Sequence<T> (FingerTree<Elem, Size>.FromEnumerable (items.Select (CreateElem)));
		}

		/// <summary>
		/// Create a sequence from ISequence.
		/// </summary>
		public static Sequence<T> FromSequence (ISequence<T> seq)
		{
			return seq is Sequence<T> ? (Sequence<T>)seq : 
				new Sequence<T> (FingerTree<Elem, Size>.FromReducible ((IReducible<Elem>)seq.Map (CreateElem)));
		}

		/// <summary>
		/// Return the empty sequence.
		/// </summary>
		public static Sequence<T> Empty
		{
			get { return new Sequence<T> (FingerTree<Elem, Size>.Empty); }
		}

		/// <summary>
		/// Is this sequence empty?
		/// </summary>
		public bool IsEmpty
		{
			get { return _tree.IsEmpty; }
		}

		/// <summary>
		/// The first item of the sequence.
		/// </summary>
		public T First
		{
			get { return _tree.First; }
		}

		/// <summary>
		/// The last item of the sequence.
		/// </summary>
		public T Last
		{
			get { return _tree.Last; }
		}

		/// <summary>
		/// The left sided (from beginning) tail of the sequence.
		/// </summary>
		public Sequence<T> RestL
		{
			get { return new Sequence<T> (_tree.RestL); }
		}

		/// <summary>
		/// The right sided (from end) tail of the sequence.
		/// </summary>
		public Sequence<T> RestR
		{
			get { return new Sequence<T> (_tree.RestR); }
		}

		/// <summary>
		/// The left sided tail.
		/// </summary>
		public IStream<T> Rest
		{
			get { return RestL; }
		}

		/// <summary>
		/// Return the left view of the sequence. Returns both the first item and
		/// left tail.
		/// </summary>
		public Tuple<T, Sequence<T>> LeftView
		{
			get
			{
				var view = _tree.LeftView ();
				return view == null ? null : 
					Tuple.Create ((T)view.First, new Sequence<T> (view.Rest));
			}
		}

		/// <summary>
		/// Return the riht view of the sequence. Returns both the last item and
		/// right tail.
		/// </summary>
		public Tuple<Sequence<T>, T> RightView
		{
			get
			{
				var view = _tree.RightView ();
				return view == null ? null : 
					Tuple.Create (new Sequence<T> (view.Rest), (T)view.Last);
			}
		}

		/// <summary>
		/// Return an item by its position in the sequence.
		/// </summary>
		public T this [int index]
		{
			get
			{
				if (index < 0 || index >= Length)
					throw new IndexOutOfRangeException ("Index was outside the bounds of the sequence.");
				return _tree.Split (FindP (index), new Size ()).Item;
			}
		}

		/// <summary>
		/// Return the length of the sequence.
		/// </summary>
		public int Length
		{
			get { return _tree.Measure (); }
		}

		/// <summary>
		/// Appends items to the beginning of the sequence.
		/// </summary>
		public Sequence<T> AppendWith (StrictList<T> items, Sequence<T> other)
		{
			return new Sequence<T> (_tree.AppendTree (items.Map (CreateElem), other._tree));
		}

		/// <summary>
		/// Split a sequence in a position indicated by the index. Returns the prefix sequence, 
		/// the item at the given position, and the postfix sequence.
		/// </summary>
		public Tuple<Sequence<T>, T, Sequence<T>> SplitAt (int index)
		{
			var split = _tree.Split (FindP (index), new Size ());
			return Tuple.Create (new Sequence<T> (split.Left), (T)split.Item, 
				new Sequence<T> (split.Right));
		}

		/// <summary>
		/// Convert to sequence to an array.
		/// </summary>
		/// <returns></returns>
		public T[] AsArray ()
		{
			var result = new T[Length];
			_tree.ReduceLeft (0, (i, e) =>
			{
				result [i] = e;
				return i + 1;
			}
			);
			return result;
		}

		/// <summary>
		/// Map this sequence to another.
		/// </summary>
		public Sequence<U> Map<U> (Func<T, U> map)
		{
			return ReduceLeft (Sequence<U>.Empty, (s, e) => s + map (e));
		}

		/// <summary>
		/// Filter this sequence according to a predicate.
		/// </summary>
		public Sequence<T> Filter (Func<T, bool> predicate)
		{
			return ReduceLeft (Empty, (s, e) => predicate (e) ? s + e : s);
		}

		/// <summary>
		/// Collect (flatten) the sequences return by a function to a single sequence.
		/// </summary>
		public Sequence<U> Collect<U> (Func<T, Sequence<U>> func)
		{
			return ReduceLeft (Sequence<U>.Empty, (s, e) => s + func (e));
		}

		/// <summary>
		/// Zip this sequence with a stream creating list of tuples. The zipping stops when
		/// either the this list or the stream runs out.
		/// </summary>
		public Sequence<Tuple<T, U>> ZipWith<U> (IStream<U> other)
		{
			return ReduceLeft (Tuple.Create (Sequence<Tuple<T, U>>.Empty, other),
				(t, e) => t.Item2.IsEmpty ? t :
					Tuple.Create (t.Item1 + Tuple.Create (e, t.Item2.First), t.Item2.Rest)
				).Item1;
		}

		#region Overridden from Object

		public override bool Equals (object obj)
		{
			return (obj is Sequence<T>) && this.IsEqualTo ((Sequence<T>)obj);
		}

		public override int GetHashCode ()
		{
			return ReduceLeft (0, (h, e) => h ^ e.GetHashCode ());
		}

		public override string ToString ()
		{
			return this.ToString ("[", "]", ", ");
		}

		#endregion

		#region Operator overloads

		public static Sequence<T> operator + (T item, Sequence<T> seq)
		{
			return new Sequence<T> (seq._tree.AddLeft (CreateElem (item)));
		}

		public static Sequence<T> operator + (Sequence<T> seq, T item)
		{
			return new Sequence<T> (seq._tree.AddRight (CreateElem (item)));
		}

		public static Sequence<T> operator + (Sequence<T> seq, Sequence<T> other)
		{
			return new Sequence<T> (seq._tree.AppendTree (StrictList<Elem>.Empty, other._tree));
		}

		#endregion

		#region ISequence<T> implementation

		ISequence<T> ISequence<T>.Concat (ISequence<T> other)
		{
			return this + Sequence<T>.FromSequence (other);
		}

		ISequence<U> ISequence<T>.Map<U> (Func<T, U> map)
		{
			return Map (map);
		}

		ISequence<T> ISequence<T>.Filter (Func<T, bool> predicate)
		{
			return Filter (predicate);
		}

		ISequence<U> ISequence<T>.Collect<U> (Func<T, ISequence<U>> func)
		{
			return Collect (e => Sequence<U>.FromSequence (func (e)));
		}

		ISequence<Tuple<T, U>> ISequence<T>.ZipWith<U> (ISequence<U> other)
		{
			return ZipWith (other);
		}

		#endregion

		#region IReducible<T> implementation

		public U ReduceLeft<U> (U acc, Func<U, T, U> func)
		{
			return _tree.ReduceLeft (acc, (a, e) => func (a, e.Value));
		}

		public U ReduceRight<U> (Func<T, U, U> func, U acc)
		{
			return _tree.ReduceRight ((e, a) => func (e.Value, a), acc);
		}

		#endregion

		#region IVisualizable implementation
	
		public Visual ToVisual ()
		{
			return _tree.ToVisual ();
		}

		#endregion
	}

	/// <summary>
	/// Static helper class for creating sequences.
	/// </summary>
	public static class Sequence
	{
		/// <summary>
		/// The builder implementation that allows creating sequences generically.
		/// </summary>
		internal class Builder<T> : IStreamBuilder<Sequence<T>, T>
		{
			public Sequence<T> Empty
			{
				get { return Sequence<T>.Empty; }
			}

			public Sequence<T> Cons (T first, Sequence<T> rest)
			{
				return Sequence.Cons (first, rest);
			}

			public Sequence<T> FromEnumerable (IEnumerable<T> items)
			{
				return Sequence.FromEnumerable (items);
			}
		}

		/// <summary>
		/// Construct a sequense from a head and tail.
		/// </summary>
		public static Sequence<T> Cons<T> (T first, Sequence<T> rest)
		{
			return first + rest;
		}

		/// <summary>
		/// Construct a singleton sequence.
		/// </summary>
		public static Sequence<T> Cons<T> (T first)
		{
			return first + Sequence<T>.Empty;
		}

		/// <summary>
		/// Construct a sequence from ISequence.
		/// </summary>
		public static Sequence<T> Create<T> (ISequence<T> seq)
		{
			return Sequence<T>.FromSequence (seq);
		}

		/// <summary>
		/// Create a sequence from enumerable.
		/// </summary>
		public static Sequence<T> FromEnumerable<T> (IEnumerable<T> items)
		{
			return Sequence<T>.FromEnumerable (items);
		}

		/// <summary>
		/// Create a sequence with given elements.
		/// </summary>
		public static Sequence<T> Create<T> (params T[] items)
		{
			return Sequence<T>.FromEnumerable (items);
		}
	}
}