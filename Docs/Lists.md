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
```
There are a couple of things to note about the above definition. Instead of using `null` to represent an empty list, there is a special singleton object that helps us maintaining the value semantics. You can query for the First and Rest of an empty list but instead of null pointer exception you will get an empty list exception. Another thing to notice is that there is a private setter for the Rest property. Although the list is immutable, the implementation of some of the list operations can be done more efficiently by mutating the list under the hood. The immutability is of course preserved from the user point of view, and there is never a case where mutation would be visible to the user. Another benefit of having the private setter for the Rest property is that it makes implementation of some of the list operations much cleaner.


