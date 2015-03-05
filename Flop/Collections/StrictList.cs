namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Flop.Visuals;

	/// <summary>
	/// An immutable linked list.
	/// </summary>
	/// <typeparam name="T">The item type of the list.</typeparam>
	public class StrictList<T> : ISequence<T>, IReducible<T>, IVisualizable
	{
		private static readonly StrictList<T> _empty = new StrictList<T> (default(T), null);
		private T _first;
		private StrictList<T> _rest;
		
		/// <summary>
		/// The first item in the list.
		/// </summary>
		public T First
		{ 
			get
			{
				if (this == _empty)
					throw new EmptyListException ();
				return _first;
			}
		}
			
		/// <summary>
		/// The rest of the list.
		/// </summary>
		public StrictList<T> Rest
		{ 
			get
			{
				if (this == _empty)
					throw new EmptyListException ();
				return _rest;
			}
			private set
			{
				if (this != _empty)
					_rest = value;
			}
		}

		/// <summary>
		/// Return an empty list.
		/// </summary>
		public static StrictList<T> Empty
		{
			get { return _empty; }
		}

		/// <summary>
		/// Is the list empty.
		/// </summary>
		public bool IsEmpty
		{
			get { return this == Empty; }
		}

		IStream<T> IStream<T>.Rest
		{
			get { return Rest; }
		}

		/// <summary>
		/// Private constructor. Use Cons to create a list.
		/// </summary>
		private StrictList (T first, StrictList<T> rest)
		{
			_first = first;
			_rest = rest;
		}

		/// <summary>
		/// Create a list by appending an item at head of the list.
		/// </summary>
		/// <param name="head">The new head item.</param>
		/// <param name="tail">The tail of the list.</param>
		public static StrictList<T> Cons (T first, StrictList<T> rest)
		{
			return new StrictList<T> (first, rest);
		}
		
		/// <summary>
		/// Construct a list from an enumerable.
		/// </summary>
		public static StrictList<T> FromEnumerable (IEnumerable<T> values)
		{
			var result = Empty;
			var last = result;
			
			foreach (T item in values)
			{
				var cons = Cons (item, Empty);
				if (result.IsEmpty)
					result = cons;
				else
					last.Rest = cons;
				last = cons;
			}
			return result;
		}

		/// <summary>
		/// Construct a list from a stream.
		/// </summary>
		public static StrictList<T> FromStream (IStream<T> seq)
		{
			if (seq is StrictList<T>)
				return seq as StrictList<T>;
			var result = Empty;

			for (var last = result; !seq.IsEmpty; seq = seq.Rest)
			{
				var cons = Cons (seq.First, Empty);
				if (result.IsEmpty)
					result = cons;
				else
					last.Rest = cons;
				last = cons;
			}
			return result;
		}

		/// <summary>
		/// Copy the list upto the given element. 
		/// </summary>
		/// <param name="stop">The tail of the list that, when encountered, will 
		/// stop the copying. If that tail is not found, the entire list is copied.</param>
		/// <returns>The copied list upto the given tail; or the entire source
		/// list, if the tail is not found.</returns>
		public Tuple<StrictList<T>, StrictList<T>> CopyUpTo (StrictList<T> stop)
		{
			StrictList<T> list = this, last = Empty, first = Empty, prevLast;
			
			while (!list.IsEmpty && list != stop)
			{
				prevLast = last;
				last = Cons (list.First, Empty);
				prevLast.Rest = last;
				if (first.IsEmpty)
					first = last;
				list = list.Rest;
			}
			return Tuple.Create (first, last);
		}

		/// <summary>
		/// Return a list with a new item inserted before the specified item.
		/// </summary>
		/// <param name="item">The item to be inserted.</param>
		/// <param name="before">The item before which the new item is inserted.</param>
		/// <returns>A new list with the item inserted before the specified item. If the before item
		/// is not found; the new item is added at the end of the list.</returns>
		public StrictList<T> InsertBefore (T item, T before)
		{
			var tail = this.FindNext (before) as StrictList<T>;
			
			return CopyUpTo (tail).Bind ((prefixFirst, prefixLast) =>
			{
				var cons = Cons (item, tail);
				prefixLast.Rest = cons;
				return prefixFirst.IsEmpty ? cons : prefixFirst;
			});
		}

		/// <summary>
		/// Remove an item from the list. 
		/// </summary>
		/// <param name="item">The item to be removed.</param>
		/// <returns>A new list without the given item.</returns>
		public StrictList<T> Remove (T item)
		{
			var tail = this.FindNext (item) as StrictList<T>;
			
			return CopyUpTo (tail).Bind ((prefixFirst, prefixLast) =>
			{
				var rest = tail.IsEmpty ? Empty : tail.Rest;
				prefixLast.Rest = rest;
				return prefixFirst.IsEmpty ? rest : prefixFirst;
			}
			);
		}

		/// <summary>
		/// Concatenate two lists together.
		/// </summary>
		public StrictList<T> Concat (StrictList<T> other)
		{
			return CopyUpTo (Empty).Bind ((prefixFirst, prefixLast) =>
			{
				prefixLast.Rest = other;
				return prefixFirst.IsEmpty ? other : prefixFirst;
			});
		}

		/// <summary>
		/// Collect the list of lists into another list.
		/// </summary>
		public StrictList<U> Collect<U> (Func<T, IStream<U>> func)
		{
			var result = StrictList<U>.Empty;
			var resultLast = result;
			
			for (var list = this; !list.IsEmpty; list = list.Rest)
			{
				for (var inner = func (list.First); !inner.IsEmpty; inner = inner.Rest)
				{
					var cons = StrictList<U>.Cons (inner.First, StrictList<U>.Empty);
					if (result.IsEmpty)
						result = cons;
					resultLast.Rest = cons;
					resultLast = cons;
				}
			}
			return result;
		}

		/// <summary>
		/// Zip this list with a stream creating list of tuples. The zipping stops when
		/// either the this list or the stream runs out.
		/// </summary>
		public StrictList<Tuple<T, U>> ZipWith<U> (IStream<U> other)
		{
			var result = StrictList<Tuple<T, U>>.Empty;
			var resultLast = result;
			
			for (var list = this; !(list.IsEmpty || other.IsEmpty); list = list.Rest,other = other.Rest)
			{
				var cons = StrictList<Tuple<T, U>>.Cons (Tuple.Create (list.First, other.First), StrictList<Tuple<T, U>>.Empty);
				if (result.IsEmpty)
				{
					result = cons;
					resultLast = cons;
				} else
				{
					resultLast.Rest = cons;
					resultLast = cons;
				}
			}
			return result;
		}

		/// <summary>
		/// Zip a list with atream extending the shorter one with default values.
		/// </summary>
		public StrictList<Tuple<T, U>> ZipExtendingWith<U> (IStream<U> other)
		{
			var result = StrictList<Tuple<T, U>>.Empty;
			var resultLast = result;
			var list = this;
			
			while (!(list.IsEmpty && other.IsEmpty))
			{
				var listItem = default(T);
				var otherItem = default(U);
				
				if (!list.IsEmpty)
				{
					listItem = list.First;
					list = list.Rest;
				}
				if (!other.IsEmpty)
				{
					otherItem = other.First;
					other = other.Rest;
				}
				var cons = StrictList<Tuple<T, U>>.Cons (Tuple.Create (listItem, otherItem), StrictList<Tuple<T, U>>.Empty);
				if (result.IsEmpty)
				{
					result = cons;
					resultLast = cons;
				} else
				{
					resultLast.Rest = cons;
					resultLast = cons;
				}
			}
			return result;
		}

		/// <summary>
		/// Left-reduces the list to a single value with a specified function 
		/// and initial value.
		/// </summary>
		/// <param name='acc'>The initial value of the accumulator.</param>
		/// <param name='func'>The function to reduce the list.</param>
		/// <typeparam name='U'>The type of the accumulator.</typeparam>
		public U ReduceLeft<U> (U acc, Func<U, T, U> func)
		{
			for (var list = this; !list.IsEmpty; list = list.Rest)
				acc = func (acc, list.First);
			return acc;
		}

		/// <summary>
		/// Right-reduces the list to a single value with a specified function 
		/// and initial value.
		/// </summary>
		/// <param name='acc'>The initial value of the accumulator.</param>
		/// <param name='func'>The function to fold the list.</param>
		/// <typeparam name='U'>The type of the accumulator.</typeparam>
		public U ReduceRight<U> (Func<T, U, U> func, U acc)
		{
			for (var list = this.Reverse<StrictList<T>, T> (); !list.IsEmpty; list = list.Rest)
				acc = func (list.First, acc);
			return acc;
		}
		
		/// <summary>
		/// Left-reduces the list with other list. The reduction is terminated 
		/// when either of the lists are exhausted.
		/// </summary>
		/// <returns>The accumulated value.</returns>
		/// <param name='acc'>Initial accumulator value.</param>
		/// <param name='func'>The function applied to the lists' items.</param>
		/// <param name='other'>The list to be reduced with.</param>
		public U ReduceWith<U, V> (U acc, IStream<V> other, Func<U, T, V, U> func)
		{
			for (var list = this; !(list.IsEmpty || other.IsEmpty); 
				 list = list.Rest, other = other.Rest)
				acc = func (acc, list.First, other.First);
			return acc;
		}

		/// <summary>
		/// Map list to another list. 
		/// </summary>
		/// <param name="func">Function that maps the items.</param>
		/// <returns>The mapped list.</returns>
		public StrictList<U> Map<U> (Func<T, U> func)
		{
			if (IsEmpty)
				return StrictList<U>.Empty;
			
			var result = List.Cons (func (First));
			var last = result;
			
			for (var list = Rest; !list.IsEmpty; list = list.Rest, last = last.Rest)
				last.Rest = List.Cons (func (list.First));
			return result;
		}

		/// <summary>
		/// Filter the list according to a predicate.
		/// </summary>
		public StrictList<T> Filter (Func<T, bool> predicate)
		{
			var result = Empty;
			var last = Empty;

			for (var list = this; !list.IsEmpty; list = list.Rest)
				if (predicate (list.First))
				{
					var tail = Cons (list.First, Empty);
					if (result.IsEmpty)
						result = tail;
					else
						last.Rest = tail;
					last = tail;
				}
			return result;
		}

		#region Overridden from Object

		/// <summary>
		/// Determines whether the specified object is equal to the current list.
		/// </summary>
		public override bool Equals (object obj)
		{
			var otherList = obj as StrictList<T>;
			return (otherList != null) && this.IsEqualTo (otherList);
		}
		
		/// <summary>
		/// Serves as a hash function for a <see cref="Flop.Collections.List`1"/> object.
		/// </summary>
		public override int GetHashCode ()
		{
			return ReduceLeft (0, (h, e) => h ^ e.GetHashCode ());
		}

		/// <summary>
		/// Returns a string representing the list.
		/// </summary>
		public override string ToString ()
		{
			return typeof (T) == typeof (char) ?
				this.ToString (null, null,  null) :
				this.ToString ("[", "]", ", ");
		}

		#endregion

		#region ISequence<T> members

		ISequence<T> ISequence<T>.Concat (ISequence<T> other)
		{
			return Concat (StrictList<T>.FromStream (other));
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
			return Collect ((Func<T, IStream<U>>)func);
		}

		ISequence<Tuple<T, U>> ISequence<T>.ZipWith<U> (ISequence<U> other)
		{
			return ZipWith (other);
		}

		#endregion

		#region IVisualizable members

		public Visual ToVisual ()
		{
			return Visual.HStack (VAlign.Top, 
				Map (item => Visual.Frame (Visual.Label (item.ToString ()), FrameKind.Rectangle)));
		}

		#endregion

		#region Operator overloads

		public static StrictList<T> operator | (T first, StrictList<T> rest)
		{
			return Cons (first, rest);
		}

		public static StrictList<T> operator + (StrictList<T> list, T item)
		{
			return list.AddToBack<StrictList<T>, T> (item);
		}

		#endregion
	}
	
	/// <summary>
	/// Static helper class for creating lists.
	/// </summary>
	public static class List
	{
		internal class Builder<T> : IStreamBuilder<StrictList<T>, T>
		{
			public StrictList<T> Empty
			{
				get { return StrictList<T>.Empty; }
			}

			public StrictList<T> Cons (T first, StrictList<T> rest)
			{
				return List.Cons (first, rest);
			}

			public StrictList<T> FromEnumerable (IEnumerable<T> items)
			{
				return List.FromEnumerable (items);
			}
		}

		/// <summary>
		/// Helper to create a cons list without explicitly specifying the item type. 
		/// </summary>
		public static StrictList<T> Cons<T> (T first, StrictList<T> rest)
		{
			return StrictList<T>.Cons (first, rest);
		}

		/// <summary>
		/// Helper to create a cons list without explicitly specifying the item type. 
		/// </summary>
		public static StrictList<T> Cons<T> (T first)
		{
			return StrictList<T>.Cons (first, StrictList<T>.Empty);
		}

		/// <summary>
		/// Constructs a new list from an array.
		/// </summary>
		/// <param name="array">The array whose items are placed into the list.</param>
		/// <returns>A new list that contains the same items in the same order as the 
		/// array.</returns>
		public static StrictList<T> FromArray<T> (T[] array)
		{
			return array.ReduceRight (StrictList<T>.Cons, StrictList<T>.Empty);
		}

		/// <summary>
		/// Create a list from a reducible structure.
		/// </summary>
		public static StrictList<T> FromReducible<T> (IRightReducible<T> reducible)
		{
			return reducible.ReduceRight (StrictList<T>.Cons, StrictList<T>.Empty);
		}

		/// <summary>
		/// Create a list from a reducible structure.
		/// </summary>
		public static StrictList<U> MapReducible<T, U> (IRightReducible<T> reducible, Func<T, U> map)
		{
			return reducible.ReduceRight ((t, l) => map (t) | l, StrictList<U>.Empty);
		}

		/// <summary>
		/// Constructs a new list from a variable argument list.
		/// </summary>
		/// <param name="items">The items that are placed into the list.</param>
		/// <returns>A new list that contains the items in the order specified.</returns>
		public static StrictList<T> Create<T> (params T[] items)
		{
			return FromArray (items);
		}

		/// <summary>
		/// Constructs a new list from IEnumerable.
		/// </summary>
		/// <param name="items">The items that are placed into the list.</param>
		/// <returns>A new list that contains the items in the order specified.</returns>
		public static StrictList<T> FromEnumerable<T> (IEnumerable<T> items)
		{
			return StrictList<T>.FromEnumerable (items);
		}
	}
}
