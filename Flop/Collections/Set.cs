namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Exception that is thrown if an empty set is accessed.
	/// </summary>
	public class EmptySetException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public EmptySetException () : base("Set is empty")
		{
		}
	}

	/// <summary>
	/// An immutable set data structure.
	/// </summary>
	/// <typeparam name="T">The value type of the map.</typeparam>
	public abstract class Set<T> : Tree<T>, IEnumerable<T>, IReducible<T> where T : IComparable<T>
	{
		/// <summary>
		/// Static constructor initializes the empty set reference.
		/// </summary>
		static Set ()
		{
			Tree<Set<T>, T>._empty = new _Empty ();
		}

		public abstract U ReduceLeft<U> (U acc, Func<U, T, U> func);
		public abstract U ReduceRight<U> (Func<T, U, U> func, U acc);

		#region Public interface

		/// <summary>
		/// Returns an empty set.
		/// </summary>
		public static Set<T> Empty
		{
			get { return Tree<Set<T>, T>._empty; }
		}

		/// <summary>
		/// Returns a map that is constructed from enumerable.
		/// </summary>
		/// <param name="items">An enumerable that gives the values to be added.</param>
		/// <returns>A set that contains the given pairs.</returns>
		public static Set<T> Create (IEnumerable<T> items)
		{
			var array = items.Select<T, Set<T>> (v => new _SetNode (v, Empty, Empty)).ToArray ();

			return Tree<Set<T>, T>.FromArray (array, false);
		}

		/// <summary>
		/// Returns a set that is constructed from an array.
		/// </summary>
		/// <param name="items">An array that gives the values to be added.</param>
		/// <returns>A set that contains the given values.</returns>
		public static Set<T> Create (params T[] items)
		{
			return Create ((IEnumerable<T>)items);
		}
		
		/// <summary>
		/// Add an item to the set.
		/// </summary>
		/// <param name="item">The item added to the set.</param>
		/// <returns>A new set that contains the given item.</returns>
		public Set<T> Add (T item)
		{
			return Contains (item) ? 
				this :
				Tree<Set<T>, T>.Add (this, new _SetNode (item, Empty, Empty));
		}

		/// <summary>
		/// Remove an item from the set.
		/// </summary>
		/// <param name="item">The item to be removed.</param>
		/// <returns>A new set that does not contain the given item.</returns>
		public Set<T> Remove (T item)
		{
			return Tree<Set<T>, T>.Remove (this, item);
		}

		/// <summary>
		/// Tests if the set contains an item.
		/// </summary>
		/// <param name="item">The item to be searched for.</param>
		/// <returns>True, if the set contains the item; false, otherwise.</returns>
		public bool Contains (T item)
		{
			return !Tree<Set<T>, T>.Search (this, item).IsEmpty ();
		}
		
		/// <summary>
		/// Return the union with another set.
		/// </summary>
		public static Set<T> operator+ (Set<T> s1, Set<T> s2)
		{
			return Set<T>.Create ((s1 as IEnumerable<T>).Concat (s2));
		}
		
		/// <summary>
		/// Return the intersection with another set.
		/// </summary>
		public static Set<T> operator* (Set<T> s1, Set<T> s2)
		{
			return Create (s1.Where (i => s2.Contains (i)));
		}
		
		/// <summary>
		/// Subtracts another set from this one.
		/// </summary>
		public static Set<T> operator- (Set<T> s1, Set<T> s2)
		{
			return Create (s1.Where (i => !s2.Contains (i)));
		}
		
		/// <summary>
		/// Returns the number of items in the map.
		/// </summary>
		public int Count
		{
			get { return Weight; }
		}

		#endregion

		/// <summary>
		/// A concrete set implementation that represents the empty set.
		/// </summary>
		private class _Empty : Set<T>
		{
			protected internal override Tree<T> Left
			{
				get { throw new EmptySetException (); }
			}

			protected internal override Tree<T> Right
			{
				get { throw new EmptySetException (); }
			}

			protected internal override T Key
			{
				get { throw new EmptySetException (); }
			}

			protected internal override int Weight
			{
				get { return 0; }
			}

			protected internal override Tree<T> Clone (Tree<T> newLeft, Tree<T> newRight, bool inPlace)
			{
				return this;
			}

			protected internal override bool IsEmpty ()
			{
				return true;
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return acc;
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return acc;
			}
		}

		/// <summary>
		/// A concrete map implementation that represents a non-empty map.
		/// </summary>
		private class _SetNode : Set<T>
		{
			private Set<T> _left;
			private Set<T> _right;
			private T _item;
			private int _weight;

			public _SetNode (T item, Set<T> left, Set<T> right)
			{
				_left = left;
				_right = right;
				_item = item;
				_weight = -1;
			}

			protected internal override Tree<T> Left
			{
				get { return _left; }
			}

			protected internal override Tree<T> Right
			{
				get { return _right; }
			}

			protected internal override T Key
			{
				get { return _item; }
			}

			protected internal override int Weight
			{
				get
				{
					if (_weight < 0)
					{
						_weight = Left.Weight + Right.Weight + 1;
					}
					return _weight;
				}
			}

			protected internal override Tree<T> Clone (Tree<T> newLeft, Tree<T> newRight, bool inPlace)
			{
				if (inPlace)
				{
					_left = (Set<T>)newLeft;
					_right = (Set<T>)newRight;
					return this;
				}
				else
					return new _SetNode (_item, (Set<T>)newLeft, (Set<T>)newRight);
			}

			protected internal override bool IsEmpty ()
			{
				return false;
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return _right.ReduceLeft (func (_left.ReduceLeft (acc, func), Key), func);
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return _left.ReduceRight (func, func (Key, _right.ReduceRight (func, acc)));
			}
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator ()
		{
			foreach (_SetNode node in Tree<Set<T>, T>.TraverseDepthFirst(this))
			{
				yield return node.Key;
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("[");
			bool outputted = false;
			
			foreach (var item in this)
			{
				if (outputted)
					sb.Append (", ");
				else
					outputted = true;
				sb.Append (item);
			}
			sb.Append ("]");
			
			return sb.ToString ();
		}
	}
}
