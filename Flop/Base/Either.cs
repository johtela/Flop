namespace Flop
{
	using System;

	public class EitherException : Exception
	{
		public EitherException (string choice) : base ("The choice '{0}' is not valid.")
		{}
	}

	public abstract class Either<T, U>
	{
		public abstract T Left { get; }
		public abstract U Right { get; }

		private class _Left : Either<T, U>
		{
			private T _left;

			public _Left (T left)
			{
				_left = left;
			}

			public override T Left
			{
				get { return _left; }
			}

			public override U Right
			{
				get { throw new EitherException ("Right"); }
			}
		}

		private class _Right : Either<T, U>
		{
			private U _right;

			public _Right (U right)
			{
				_right = right;
			}

			public override T Left
			{
				get { throw new EitherException ("Left"); }
			}

			public override U Right
			{
				get { return _right; }
			}
		}

		public static Either<T, U> Create (T value)
		{
			return new _Left (value);
		}

		public static Either<T, U> Create (U value)
		{
			return new _Right (value);
		}

		public V Match<V> (Func<T, V> left, Func<U, V> right)
		{
			if (this is _Left)
				return left (Left);
			else
				return right (Right);
		}

		public bool IsLeft
		{
			get { return this is _Left; }
		}

		public bool IsRight
		{
			get { return this is _Right; }
		}
	}
}
