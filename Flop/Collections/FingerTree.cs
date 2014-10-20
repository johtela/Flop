namespace Flop.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Flop.Visuals;

	/// <summary>
	/// Tree empty exception.
	/// </summary>
	public class EmptyTreeException : Exception
	{
		public EmptyTreeException () : base ("Tree is empty.")
		{
		}
	}

	/// <summary>
	/// The inner node of the finger tree has either degree two or three.
	/// </summary>
	public abstract class Node<T, V> : IReducible<T>, IMeasurable<V>, IVisualizable
		where T : IMeasurable<V>
		where V : IMonoid<V>, new ()
	{
		public readonly V Value;

		public Node (V value)
		{
			Value = value;
		}
		
		public V Measure ()
		{
			return Value;
		}

		public abstract Digit<T, V> ToDigit ();
		public abstract U ReduceLeft<U> (U acc, Func<U, T, U> func);
		public abstract U ReduceRight<U> (Func<T, U, U> func, U acc);

		private sealed class Node2 : Node<T, V>
		{
			public readonly T Item1;
			public readonly T Item2;

			public Node2 (T item1, T item2) :
				base (item1.Measure ().Plus (item2.Measure ()))
			{
				Item1 = item1;
				Item2 = item2;
			}

			public override Digit<T, V> ToDigit ()
			{
				return new Digit<T, V> (Item1, Item2);
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return func (func (acc, Item1), Item2);
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return func (Item1, func (Item2, acc));
			}
		}

		private sealed class Node3 : Node<T, V>
		{
			public readonly T Item1;
			public readonly T Item2;
			public readonly T Item3;

			public Node3 (T item1, T item2, T item3) :
				base (item1.Measure ().Plus (item2.Measure ()).Plus (item3.Measure ()))
			{
				Item1 = item1;
				Item2 = item2;
				Item3 = item3;
			}

			public override Digit<T, V> ToDigit ()
			{
				return new Digit<T, V> (Item1, Item2, Item3);
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return func (func (func (acc, Item1), Item2), Item3);
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return func (Item1, func (Item2, func (Item3, acc)));
			}
		}

		public static Node<T, V> Create (T item1, T item2)
		{
			return new Node2 (item1, item2);
		}

		public static Node<T, V> Create (T item1, T item2, T item3)
		{
			return new Node3 (item1, item2, item3);
		}

		public static StrictList<Node<T, V>> CreateMany (StrictList<T> items)
		{
			switch (items.Length ())
			{
				case 0:
				case 1:
					throw new ArgumentException ("List should contain at least two items.");
				case 2:
					return List.Cons (Create (items.First, items.Rest.First));
				case 3:
					return List.Cons (Create (items.First, items.Rest.First, items.Rest.Rest.First));
				case 4:
					return List.Create (Create (items.First, items.Rest.First), 
					Create (items.Rest.Rest.First, items.Rest.Rest.Rest.First));
				default:
					return Create (items.First, items.Rest.First, items.Rest.Rest.First) |
						CreateMany (items.Rest.Rest.Rest);
			}
		}

		#region IVisualizable implementation
		
		public Visual ToVisual ()
		{
			return Visual.HStack (VAlign.Top, List.MapReducible (this, FrameNode));
		}

		private Visual FrameNode (T value)
		{
			return Visual.Margin (Visual.Frame (Visual.Margin (Visual.Visualize (value), 2, 2, 2, 2),
				FrameKind.RoundRectangle), 4, 4, 4, 4);
		}

		#endregion	
	}

	/// <summary>
	/// The front and back parts of the tree have one to four items in an array.
	/// </summary>
	public class Digit<T, V> : IReducible<T>, IMeasurable<V>, ISplittable<StrictList<T>, T, V>, 
		IVisualizable
		where T : IMeasurable<V>
		where V : IMonoid<V>, new ()
	{
		private readonly T[] _items;

		public Digit (StrictList<T> items)
		{
			var len = items.Length ();
			if (len < 1 || len > 4)
				throw new ArgumentException ("Digit array must have length of 1..4");
			_items = new T[len];
			for (int i = 0; i < len; i++, items = items.Rest)
				_items [i] = items.First;
		}

		public Digit (T item1)
		{
			_items = new T[] { item1 };
		}

		public Digit (T item1, T item2)
		{
			_items = new T[] { item1, item2 };
		}

		public Digit (T item1, T item2, T item3)
		{
			_items = new T[] { item1, item2, item3 };
		}

		public Digit (T item1, T item2, T item3, T item4)
		{
			_items = new T[] { item1, item2, item3, item4 };
		}

		private StrictList<T> Slice (int start, int end)
		{
			var result = StrictList<T>.Empty;
			for (int i = end; i >= start; i--)
				result = _items [i] | result;
			return result;
		}

		public Split<StrictList<T>, T, V> Split (Func<V, bool> predicate, V acc)
		{
			var i = 0;
			do
			{
				acc = acc.Plus (_items [i].Measure ());
			}
			while (!predicate (acc) && ++i < _items.Length);

			return new Split<StrictList<T>, T, V> (
				Lazy.Create (Fun.Partial (Slice, 0, i - 1)),
				_items [i],
				Lazy.Create (Fun.Partial (Slice, i + 1, _items.Length - 1)));
		}

		public static Digit<T, V> operator + (T item, Digit<T, V> digit)
		{
			switch (digit._items.Length)
			{
				case 1:
					return new Digit<T, V> (item, digit [0]);
				case 2:
					return new Digit<T, V> (item, digit [0], digit [1]);
				case 3:
					return new Digit<T, V> (item, digit [0], digit [1], digit [2]);
				default:
					throw new ArgumentException ("Digit is full", "digit");
			}
		}

		public static Digit<T, V> operator + (Digit<T, V> digit, T item)
		{
			switch (digit._items.Length)
			{
				case 1:
					return new Digit<T, V> (digit [0], item);
				case 2:
					return new Digit<T, V> (digit [0], digit [1], item);
				case 3:
					return new Digit<T, V> (digit [0], digit [1], digit [2], item);
				default:
					throw new ArgumentException ("Digit is full", "digit");
			}
		}

		public T this [int index]
		{
			get { return _items [index]; }
		}

		public bool IsFull
		{
			get { return _items.Length > 3; }
		}

		public T First
		{
			get { return _items [0]; }
		}

		public T Last
		{
			get { return _items [_items.Length - 1]; }
		}

		public StrictList<T> Prefix
		{
			get { return Slice (0, _items.Length - 2); }
		}

		public StrictList<T> Suffix
		{
			get { return Slice (1, _items.Length - 1); }
		}

		#region IMeasurable<V> implementation

		public V Measure ()
		{
			return ReduceLeft (new V (), (v, i) => v.Plus (i.Measure ()));
		}

		#endregion

		#region IReducible<T> implementation

		public U ReduceLeft<U> (U acc, Func<U, T, U> func)
		{
			return _items.ReduceLeft (acc, func);
		}

		public U ReduceRight<U> (Func<T, U, U> func, U acc)
		{
			return _items.ReduceRight (func, acc);
		}

		#endregion

		#region IVisualizable implementation

		public Visual ToVisual ()
		{
			return Visual.HStack (VAlign.Top, List.MapReducible (this, FrameNode));
		}

		private Visual FrameNode (T value)
		{
			return Visual.Frame (Visual.Margin (Visual.Visualize (value), 2, 2, 2, 2),
				FrameKind.Rectangle);
		}

		#endregion
	}

	/// <summary>
	/// Left view of the tree.
	/// </summary>
	public class ViewL<T, V>
		where T : IMeasurable<V>
		where V : IMonoid<V>, new ()
	{
		public readonly T First;
		public readonly FingerTree<T, V> Rest;

		public ViewL (T first, FingerTree<T, V> rest)
		{
			First = first;
			Rest = rest;
		}
	}

	/// <summary>
	/// Rigth view of the tree.
	/// </summary>
	public class ViewR<T, V>
		where T : IMeasurable<V>
		where V : IMonoid<V>, new ()
	{
		public readonly T Last;
		public readonly FingerTree<T, V> Rest;

		public ViewR (T last, FingerTree<T, V> rest)
		{
			Last = last;
			Rest = rest;
		}
	}

	/// <summary>
	/// The finger tree is either empty, contains a single item, or then it contains
	/// front, inner, and back parts where the inner part can be empty.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class FingerTree<T, V> : IReducible<T>, IMeasurable<V>, 
		ISplittable<FingerTree<T, V>, T, V>, IVisualizable
		where T : IMeasurable<V>
		where V : IMonoid<V>, new ()
	{
		public abstract FingerTree<T, V> AddLeft (T leftItem);
		public abstract FingerTree<T, V> AddRight (T rightItem);
		public abstract ViewL<T, V> LeftView ();
		public abstract ViewR<T, V> RightView ();
		public abstract FingerTree<T, V> AppendTree (IReducible<T> items, FingerTree<T, V> tree);

		#region IReducible<T> implementation
		public abstract U ReduceLeft<U> (U acc, Func<U, T, U> func);
		public abstract U ReduceRight<U> (Func<T, U, U> func, U acc);
		#endregion

		#region IMeasurable<V> implementation
		public abstract V Measure ();
		#endregion

		#region ISplittable<FingerTree<T, V>, T, V> implementation
		public abstract Split<FingerTree<T, V>, T, V> Split (Func<V, bool> predicate, V acc);
		#endregion

		#region IVisualizable implementation
		public abstract Visual ToVisual ();

		private Visual EmptyNode ()
		{
			return Visual.Margin (Visual.Frame (Visual.Label ("-"), FrameKind.Ellipse), 10, 10, 0, 0);
		}
		#endregion

		private static FingerTree<T, V> _empty = new _Empty ();

		/// <summary>
		/// An empty tree.
		/// </summary>
		private sealed class _Empty : FingerTree<T, V>
		{
			public _Empty ()
			{
			}

			public override FingerTree<T, V> AddLeft (T leftItem)
			{
				return new _Single (leftItem);
			}

			public override FingerTree<T, V> AddRight (T rightItem)
			{
				return new _Single (rightItem);
			}

			public override ViewL<T, V> LeftView ()
			{
				return null;
			}

			public override ViewR<T, V> RightView ()
			{
				return null;
			}

			public override FingerTree<T, V> AppendTree (IReducible<T> items, FingerTree<T, V> tree)
			{
				return tree.Prepend (items);
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return acc;
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return acc;
			}

			public override V Measure ()
			{
				return new V ();
			}

			public override Split<FingerTree<T, V>, T, V> Split (Func<V, bool> predicate, V acc)
			{
				throw new EmptyTreeException ();
			}

			public override Visual ToVisual ()
			{
				return EmptyNode ();
			}
		}

		/// <summary>
		/// Tree with a single item.
		/// </summary>
		private sealed class _Single : FingerTree<T, V>
		{
			public readonly T Item;

			public _Single (T item)
			{
				Item = item;
			}

			public override FingerTree<T, V> AddLeft (T leftItem)
			{
				return new _Deep (new Digit<T, V> (leftItem),
					new FingerTree<Node<T, V>, V>._Empty (),
					new Digit<T, V> (Item));
			}

			public override FingerTree<T, V> AddRight (T rightItem)
			{
				return new _Deep (new Digit<T, V> (Item),
					new FingerTree<Node<T, V>, V>._Empty (),
					new Digit<T, V> (rightItem));
			}

			public override ViewL<T, V> LeftView ()
			{
				return new ViewL<T, V> (Item, _empty);
			}

			public override ViewR<T, V> RightView ()
			{
				return new ViewR<T, V> (Item, _empty);
			}

			public override FingerTree<T, V> AppendTree (IReducible<T> items, 
				FingerTree<T, V> tree)
			{
				return tree.Prepend (items).AddLeft (Item);
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return func (acc, Item);
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return func (Item, acc);
			}

			public override V Measure ()
			{
				return Item.Measure ();
			}

			public override Split<FingerTree<T, V>, T, V> Split (Func<V, bool> predicate, V acc)
			{
				return new Split<FingerTree<T, V>, T, V> (Lazy.Create (_empty), Item,
					Lazy.Create (_empty));
			}

			public override Visual ToVisual ()
			{
				return Visual.Frame (Visual.Margin (Visual.Visualize (Item), 2, 2, 2, 2), 
					FrameKind.Ellipse);
			}
		}

		/// <summary>
		/// Deep tree with a front and back digits plus the inner tree.
		/// </summary>
		private sealed class _Deep : FingerTree<T, V>
		{
			public readonly V Value;
			public readonly Digit<T, V> Front;
			public readonly FingerTree<Node<T, V>, V> Inner;
			public readonly Digit<T, V> Back;

			// Memoized views.
			private ViewL<T, V> _leftView;
			private ViewR<T, V> _rightView;

			public _Deep (Digit<T, V> front, FingerTree<Node<T, V>, V> inner, Digit<T, V> back)
			{
				Front = front;
				Inner = inner;
				Back = back;
				Value = Front.Measure ().Plus (Inner.Measure ().Plus (Back.Measure ()));
			}

			public override FingerTree<T, V> AddLeft (T leftItem)
			{
				return Front.IsFull ?
					new _Deep (new Digit<T, V> (leftItem, Front [0]),
						Inner.AddLeft (Node<T, V>.Create (Front [1], Front [2], Front [3])),
						Back) :
					new _Deep (leftItem + Front, Inner, Back);
			}

			public override FingerTree<T, V> AddRight (T rightItem)
			{
				return Back.IsFull ?
					new _Deep (Front,
						Inner.AddRight (Node<T, V>.Create (Back [0], Back [1], Back [2])),
						new Digit<T, V> (Back [3], rightItem)) :
					new _Deep (Front, Inner, Back + rightItem);
			}

			public override ViewL<T, V> LeftView ()
			{
				return Fun.Memoize (() => new ViewL<T, V> (Front.First, DeepL (Front.Suffix, Inner, Back)), 
					ref _leftView);
			}

			public override ViewR<T, V> RightView ()
			{
				return Fun.Memoize (() => new ViewR<T, V> (Back.Last, DeepR (Front, Inner, Back.Prefix)),
					ref _rightView);
			}

			public override FingerTree<T, V> AppendTree (IReducible<T> items, FingerTree<T, V> tree)
			{
				if (tree is _Empty)
					return Append (items);
				if (tree is _Single)
					return Append (items).AddRight ((tree as _Single).Item);
				var other = tree as _Deep;
				var innerItems = List.FromReducible (Back.RightConcat (items).RightConcat (other.Front));
				return new _Deep (Front,
					Inner.AppendTree (Node<T, V>.CreateMany (innerItems), other.Inner),
					other.Back);
			}

			public override U ReduceLeft<U> (U acc, Func<U, T, U> func)
			{
				return Back.ReduceLeft (Inner.ReduceLeft (
					Front.ReduceLeft (acc, func), (a, n) => n.ReduceLeft (a, func)), func);
			}

			public override U ReduceRight<U> (Func<T, U, U> func, U acc)
			{
				return Front.ReduceRight (func, Inner.ReduceRight ((n, a) => n.ReduceRight (func, a), 
					Back.ReduceRight (func, acc))
				);
			}

			public override V Measure ()
			{
				return Value;
			}

			public override Split<FingerTree<T, V>, T, V> Split (Func<V, bool> predicate, V acc)
			{
				var vfront = acc.Plus (Front.Measure ());
				var vinner = vfront.Plus (Inner.Measure ());	

				if (predicate (vfront))
				{
					var split = Front.Split (predicate, acc);
					return new Split<FingerTree<T, V>, T, V> (
						Lazy.Create (Fun.Partial (FromReducible, split.Left)),
						split.Item,
						Lazy.Create (Fun.Partial (DeepL, split.Right, Inner, Back)));
				}
				else if (predicate (vinner))
				{
					var innerSplit = Inner.Split (predicate, vfront);
					var digitSplit = innerSplit.Item.ToDigit ().Split (
						predicate, vfront.Plus (innerSplit.Left.Measure ()));
					return new Split<FingerTree<T, V>, T, V> (
						Lazy.Create (Fun.Partial (DeepR, Front, innerSplit.Left, digitSplit.Left)),
						digitSplit.Item,
						Lazy.Create (Fun.Partial (DeepL, digitSplit.Right, innerSplit.Right, Back)));
				}
				else
				{
					var split = Back.Split (predicate, vinner);
					return new Split<FingerTree<T, V>, T, V> (
						Lazy.Create (Fun.Partial (DeepR, Front, Inner, split.Left)),
						split.Item,
						Lazy.Create (Fun.Partial (FromReducible, split.Right)));
				}
			}

			public override Visual ToVisual ()
			{
				return Visual.VStack (HAlign.Center,
					Visual.HStack (VAlign.Center, Front.ToVisual (), EmptyNode (), Back.ToVisual ()),
					Visual.Margin (Inner.ToVisual (), 0, 0, 10, 0));
			}
		}

		public static FingerTree<T, V> Empty
		{
			get { return _empty; }
		}

		public bool IsEmpty
		{
			get { return this is _Empty; }
		}

		public T First
		{
			get { return CheckView (LeftView ()).First; }
		}

		public T Last
		{
			get { return CheckView (RightView ()).Last; }
		}

		public FingerTree<T, V> RestL
		{
			get { return CheckView (LeftView ()).Rest; }
		}

		public FingerTree<T, V> RestR
		{
			get { return CheckView (RightView ()).Rest; }
		}

		public FingerTree<T, V> Prepend (IReducible<T> items)
		{
			return items.ReduceRight ((i, t) => t.AddLeft (i), this);
		}

		public FingerTree<T, V> Prepend (IEnumerable<T> items)
		{
			return items.Reverse ().Aggregate (this, (t, i) => t.AddLeft (i));
		}

		public FingerTree<T, V> Append (IReducible<T> items)
		{
			return items.ReduceLeft (this, (t, i) => t.AddRight (i));
		}

		public FingerTree<T, V> Append (IEnumerable<T> items)
		{
			return items.Aggregate (this, (t, i) => t.AddRight (i));
		}

		public static FingerTree<T, V> FromReducible (IReducible<T> items)
		{
			return _empty.Append (items);
		}

		public static FingerTree<T, V> FromEnumerable (IEnumerable<T> items)
		{
			return _empty.Append (items);
		}

		public FingerTree<T, V> Concat (FingerTree<T, V> other)
		{
			return this.AppendTree (StrictList<T>.Empty, other);
		}

		private ViewL<T, V> CheckView (ViewL<T, V> viewl)
		{
			if (viewl == null)
				throw new EmptyTreeException ();
			return viewl;
		}

		private ViewR<T, V> CheckView (ViewR<T, V> viewr)
		{
			if (viewr == null)
				throw new EmptyTreeException ();
			return viewr;
		}

		private static FingerTree<T, V> DeepL (StrictList<T> front, FingerTree<Node<T, V>, V> inner,
			Digit<T, V> back)
		{
			if (front.IsEmpty)
			{
				var viewl = inner.LeftView ();
				return viewl == null ?
					FromReducible (back) :
					new _Deep (viewl.First.ToDigit (), viewl.Rest, back);
			}
			return new _Deep (new Digit<T, V> (front), inner, back);
		}

		private static FingerTree<T, V> DeepR (Digit<T, V> front, FingerTree<Node<T, V>, V> inner, 
			StrictList<T> back)
		{
			if (back.IsEmpty)
			{
				var viewr = inner.RightView ();
				return viewr == null ?
					FromReducible (front) :
					new _Deep (front, viewr.Rest, viewr.Last.ToDigit ());
			}
			return new _Deep (front, inner, new Digit<T, V> (back));
		}
	}
}
