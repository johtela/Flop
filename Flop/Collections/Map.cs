namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Exception that is thrown if an empty map is accessed.
	/// </summary>
	public class EmptyMapException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public EmptyMapException () : base("Map is empty") { }
	}

	/// <summary>
	/// An immutable map data structure.
	/// </summary>
	/// <typeparam name="K">The key type of the map.</typeparam>
	/// <typeparam name="V">The value type of the map.</typeparam>
	public abstract class Map<K, V> : Tree<K>, IEnumerable<Tuple<K, V>>, IReducible<Tuple<K, V>>
		where K : IComparable<K>
	{
		/// <summary>
		/// Static constructor initializes the empty map reference.
		/// </summary>
		static Map ()
		{
			Tree<Map<K, V>, K>._empty = new _Empty ();
		}

		/// <summary>
		/// Abstract method that gives the value attached to the tree node.
		/// </summary>
		protected internal abstract V Value { get; }

		public abstract U ReduceLeft<U> (U acc, Func<U, Tuple<K, V>, U> func);
		public abstract U ReduceRight<U> (Func<Tuple<K, V>, U, U> func, U acc);

		#region Public interface

		/// <summary>
		/// Returns an empty map.
		/// </summary>
		public static Map<K, V> Empty
		{
			get { return Tree<Map<K, V>, K>._empty; }
		}

		/// <summary>
		/// Returns a map that is constructed from key-value pairs.
		/// </summary>
		/// <param name="pairs">An enumerable that gives the key-value pairs to be added.
		/// </param>
		/// <returns>A map that contains the given pairs.</returns>
		public static Map<K, V> FromPairs (IEnumerable<Tuple<K, V>> pairs)
		{
			var array = pairs.Select<Tuple<K, V>, Map<K, V>> (
				pair => new _MapNode (pair.Item1, pair.Item2, Empty, Empty)).ToArray ();

			return Tree<Map<K, V>, K>.FromArray (array, true);
		}

		/// <summary>
		/// Returns a map that is constructed from key-value pairs.
		/// </summary>
		/// <param name="pairs">An array that gives the key-value pairs to be added.
		/// </param>
		/// <returns>A map that contains the given pairs.</returns>
		public static Map<K, V> FromPairs (params Tuple<K, V>[] pairs)
		{
			return FromPairs ((IEnumerable<Tuple<K, V>>)pairs);
		}
		
		/// <summary>
		/// Add a new key and value to the map.
		/// </summary>
		/// <param name="key">The key to be added.</param>
		/// <param name="value">The value attached to the key.</param>
		/// <returns>A new map that contains the new key and value.</returns>
		public Map<K, V> Add (K key, V value)
		{
			if (Contains(key))
				throw new ArgumentException("Duplicate key: " + key);
			
			return Tree<Map<K, V>, K>.Add (this, new _MapNode (key, value, Empty, Empty));
		}

		/// <summary>
		/// Remove a key from the map.
		/// </summary>
		/// <param name="key">The key to be removed.</param>
		/// <returns>A new map that does not contain the given key.</returns>
		public Map<K, V> Remove (K key)
		{
			return Tree<Map<K, V>, K>.Remove (this, key);
		}

		/// <summary>
		/// Replace the value of a key. 
		/// </summary>
		/// <param name="key">The key whose value is changed.</param>
		/// <param name="value">The new value.</param>
		/// <returns>A new map that with the key value changed.</returns>
		public Map<K, V> Replace (K key, V value)
		{
			return Tree<Map<K, V>, K>.Replace(this, key, new _MapNode(key, value, Empty, Empty));
		}

		/// <summary>
		/// Tests if the map contains a key.
		/// </summary>
		/// <param name="key">The key to be searched for.</param>
		/// <returns>True, if the map contains the key; false, otherwise.</returns>
		public bool Contains (K key)
		{
			return !Tree<Map<K, V>, K>.Search (this, key).IsEmpty ();
		}

		public Option<V> TryGetValue (K key)
		{
			var node = Tree<Map<K, V>, K>.Search(this, key);

			return node.IsEmpty() ?
				new Option<V> () :
				new Option<V>(node.Value);
		}

		/// <summary>
		/// Returns the value associated with the key.
		/// </summary>
		/// <param name="key">The key to be searched for.</param>
		/// <returns>The value associated to the key.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the given
		/// key is not present in the map.</exception>
		public V this [K key]
		{
			get
			{
				var node = Tree<Map<K, V>, K>.Search (this, key);

				if (node.IsEmpty ())
					throw new KeyNotFoundException ("Key not found: " + key.ToString ());
				return node.Value;
			}
		}

		/// <summary>
		/// Returns the number of items in the map.
		/// </summary>
		public int Count
		{
			get { return Weight; }
		}

		/// <summary>
		/// Enumerates the keys of the map in the correct order.
		/// </summary>
		public IEnumerable<K> Keys
		{
			get
			{
				return from node in Tree<Map<K, V>, K>.TraverseDepthFirst(this)
					   select node.Key;
			}
		}

		/// <summary>
		/// Enumerates the values in the map in the order determined by the keys.
		/// </summary>
		public IEnumerable<V> Values
		{
			get
			{
				return from node in Tree<Map<K, V>, K>.TraverseDepthFirst(this)
                       select node.Value;
			}
		}

		#endregion

		/// <summary>
		/// A concrete map implementation that represents the empty map.
		/// </summary>
		private class _Empty : Map<K, V>
		{
			protected internal override Tree<K> Left
			{
				get { throw new EmptyMapException (); }
			}

			protected internal override Tree<K> Right
			{
				get { throw new EmptyMapException (); }
			}

			protected internal override K Key
			{
				get { throw new EmptyMapException (); }
			}

			protected internal override V Value
			{
				get { throw new EmptyMapException (); }
			}

			protected internal override int Weight
			{
				get { return 0; }
			}

			protected internal override Tree<K> Clone (Tree<K> newLeft, Tree<K> newRight, bool inPlace)
			{
				return this;
			}

			protected internal override bool IsEmpty ()
			{
				return true;
			}

			public override U ReduceLeft<U> (U acc, Func<U, Tuple<K, V>, U> func)
			{
				return acc;
			}

			public override U ReduceRight<U> (Func<Tuple<K, V>, U, U> func, U acc)
			{
				return acc;
			}
		}

		/// <summary>
		/// A concrete map implementation that represents a non-empty map.
		/// </summary>
		private class _MapNode : Map<K, V>
		{
			private Map<K, V> _left;
			private Map<K, V> _right;
			private K _key;
			private V _value;
			private int _weight;

			public _MapNode (K key, V value, Map<K, V> left, Map<K, V> right)
			{
				_left = left;
				_right = right;
				_key = key;
				_value = value;
				_weight = -1;
			}

			protected internal override Tree<K> Left
			{
				get { return _left; }
			}

			protected internal override Tree<K> Right
			{
				get { return _right; }
			}

			protected internal override K Key
			{
				get { return _key; }
			}

			protected internal override V Value
			{
				get { return _value; }
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

			protected internal override Tree<K> Clone (Tree<K> newLeft, Tree<K> newRight, bool inPlace)
			{
				if (inPlace)
				{
					_left = (Map<K, V>)newLeft;
					_right = (Map<K, V>)newRight;
					return this;
				}
				else
					return new _MapNode (_key, _value, (Map<K, V>)newLeft, (Map<K, V>)newRight);
			}

			protected internal override bool IsEmpty ()
			{
				return false;
			}

			public override U ReduceLeft<U> (U acc, Func<U, Tuple<K, V>, U> func)
			{
				return _right.ReduceLeft (func (_left.ReduceLeft (acc, func),
					Tuple.Create (_key, _value)), func);
			}

			public override U ReduceRight<U> (Func<Tuple<K, V>, U, U> func, U acc)
			{
				return _left.ReduceRight(func, 
					func (Tuple.Create(Key, Value), _right.ReduceRight(func, acc)));
			}
		}

		#region IEnumerable<Tuple<K,V>> Members

		/// <summary>
		/// Enumerate the key-value pairs in the map.
		/// </summary>
		/// <returns>The enumeration that contains all the key-value pairs in the map
		/// in the order determined by the keys.</returns>
		public IEnumerator<Tuple<K, V>> GetEnumerator ()
		{
			return (from node in Tree<Map<K, V>, K>.TraverseDepthFirst(this)
                   select new Tuple<K, V>(node.Key, node.Value)).GetEnumerator ();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Enumerate the key-value pairs in the map.
		/// </summary>
		/// <returns>The enumeration that contains all the key-value pairs in the map
		/// in the order determined by the keys.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}
