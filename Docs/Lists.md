Lists
=====

Programming in functional style would be quite difficult without having some basic collection types available. Functional collections are by design immutable which means they are never changed in-place. Instead, a new collection is created whenever they are modified. Effectively, multiple versions of a collection are kept in-memory at a same time. The cost of this would be, of course, prohibitive if big part of the old data structure was not reused. In practice all the immutable collections rely on some kind of structural sharing. The easiest way to achieve this is to use a tree-like, recursive data structure which consists of sub-parts of the same form as the collection itself.

The simplest example of such a data structure is a singly linked list. It consists of a head (first) element and a tail list which is the recursive part of the data structure. Tail is another list with it's own head and tail parts. In order to terminate the list, we need also a special object representing an empty list. If the tail contains an empty list, we know that head is the last element of the overall list.

In functional languages list type can be defined very consicely with as an algebraic data type:
```FSharp
type List<'a> = 
    | Nil
    | Cons of 'a * List<'a>
```
The names "Nil" nad "Cons" originate from Lisp which was the first programming language to demonstrate the generality of this data structure. Lisp programs themselves are just lists of symbols, and all the data structures provided by Lisp are constructed from the same "Cons" cells.

The type definition above would be enough to implement list in F#. In C# we need a little more code to define the data structure. We need to map the algebraic data type to a class. Note that we are using a different names to denote head and tail; the head element is called First and the tail list Rest. This is to make it as clear as possible for the users what these properties mean.
```Csharp
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
		/// Private constructor. Use Cons to create a list.
		/// </summary>
		private StrictList (T first, StrictList<T> rest)
		{
			_first = first;
			_rest = rest;
		}
```
Why is the class called StrictList instead just List? There are couple of reasons for that: firstly to avoid a name conflict with the `List<T>` class defined in the System.Collections.Generic namespace and secondly because there is another immutable list type in Flop library called `LazyList` which has the same principal structure but traverses its items lazily. The opposite of lazy evalutation strategy is strict, and the name of the class reflects that.

There are other things to note about the above definition as well. Instead of using `null` to represent the empty list, there is a special singleton object that helps us to maintain value semantics. You can query for the First and Rest of the empty list but instead of null pointer exception you will get an empty list exception. Another thing to notice is that there is a private setter for the Rest property. Although the list is immutable, the implementation of some of the list operations can be done more efficiently by mutating the list under the hood. The immutability is of course preserved from the user point of view, and there is never a case where mutation would be visible to the user. Another benefit of having the private setter for the Rest property is that it makes implementation of some of the list operations much cleaner.

List is usually grown by appending items at the beginning of the list. For this we define the Cons operation.
```Csharp
		/// <summary>
		/// Create a list by appending an item at head of the list.
		/// </summary>
		/// <param name="head">The new head item.</param>
		/// <param name="tail">The tail of the list.</param>
		public static StrictList<T> Cons (T first, StrictList<T> rest)
		{
			return new StrictList<T> (first, rest);
		}
```
It would be quite unefficient to construct lists just by using Cons, because adding items to the end requires traversing the list. For example, when creating a list from `IEnumerable` we would have to add items to the list first in the reverse order and then swap the order of items after all of them have been added. Although this is still an O(n) operation, it would require creating a temporary list incurring costs at the garbage collection time. Instead, we will use the private setter to grow the list from the end. Since the list is not visible to the caller until the method returns, mutating the list is safe.
```Csharp
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
```
Whenever we want to insert an item to an arbitrary location in a list, we need to make a copy of the items before that location; we can only reuse the tail. For this purpose, there is a helper method that copies the items up to specified location in the list. It returns the first and the last item of the copy of the list prefix.
```Csharp
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
```
With the help of this method operations like `InsertBefore`, `Remove`, and `Concat` can be implemented in the most efficient way possible.

Higher Order List Operations
----------------------------

