namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// An immutable view of a regular System.Array.
	/// </summary>
	public class ArraySegment<T> : IEnumerable<T>, IReducible<T>
	{
		private readonly T[] _array;
		private readonly int _first, _count;
		
		public ArraySegment (T[] array)
		{
			_array = array;
			_first = 0;
			_count = array.Length;
		}
		
		public ArraySegment (T[] array, int count)
		{
			if (count < 0 || count > array.Length)
				throw new ArgumentException ("Count is out of array index range", "count");
			_array = array;
			_first = 0;
			_count = count;
		}
		
		public ArraySegment (T[] array, int first, int count)
		{
			if (first < 0 || first >= array.Length)
				throw new ArgumentException ("First is out of array index range", "first");
			if (count < 0 || (first + count) > array.Length)
				throw new ArgumentException ("Count is out of array index range", "count");
			_array = array;
			_first = first;
			_count = count;
		}
		
		public T[] CopyToArray ()
		{
			var result = new T[_count];
			Array.Copy (_array, _first, result, 0, _count);
			return result;
		}

		public T this [int index]
		{
			get
			{
				if (index < 0 || index >= _count)
					throw new IndexOutOfRangeException ();
				return _array [index - _first]; 
			}
		}
		
		public int Length
		{
			get { return _count; }
		}
		
		#region IEnumerable[T] implementation
		
		public IEnumerator<T> GetEnumerator ()
		{
			for (int i = _first; i < (_first + _count); i++)
			{
				yield return _array[i];
			}
		}
		
		#endregion

		#region IEnumerable implementation
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		#endregion

		#region IRecducible<T> implementation

		public U ReduceLeft<U> (U acc, Func<U, T, U> func)
		{
			for (int i = _first; i < _first + _count; i++)
				acc = func (acc, _array[i]);
			return acc;
		}

		public U ReduceRight<U> (Func<T, U, U> func, U acc)
		{
			for (int i = _first + _count - 1; i >= _first; i--)
				acc = func (_array[i], acc);
			return acc;
		}

		#endregion	
	}
}