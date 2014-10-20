namespace Flop
{
	using System;

	/// <summary>
	/// Extension methods for functions.
	/// </summary>
	public static class Fun
	{
		public static Func<TRes> Partial<T, TRes> (
			this Func<T, TRes> func, T arg)
		{
			return () => func (arg);
		}

		public static Func<T2, TRes> Partial<T1, T2, TRes> (
			this Func<T1, T2, TRes> func, T1 arg1)
		{
			return arg2 => func (arg1, arg2);
		}

		public static Func<TRes> Partial<T1, T2, TRes> (
			this Func<T1, T2, TRes> func, T1 arg1, T2 arg2)
		{
			return () => func (arg1, arg2);
		}

		public static Func<T2, T3, TRes> Partial<T1, T2, T3, TRes> (
			this Func<T1, T2, T3, TRes> func, T1 arg1)
		{
			return (arg2, arg3) => func (arg1, arg2, arg3);
		}

		public static Func<T3, TRes> Partial<T1, T2, T3, TRes> (
			this Func<T1, T2, T3, TRes> func, T1 arg1, T2 arg2)
		{
			return arg3 => func (arg1, arg2, arg3);
		}

		public static Func<TRes> Partial<T1, T2, T3, TRes> (
			this Func<T1, T2, T3, TRes> func, T1 arg1, T2 arg2, T3 arg3)
		{
			return () => func (arg1, arg2, arg3);
		}

		public static Func<T2, T3, T4, TRes> Partial<T1, T2, T3, T4, TRes> (
			this Func<T1, T2, T3, T4, TRes> func, T1 arg1)
		{
			return (arg2, arg3, arg4) => func (arg1, arg2, arg3, arg4);
		}

		public static Func<T3, T4, TRes> Partial<T1, T2, T3, T4, TRes> (
			this Func<T1, T2, T3, T4, TRes> func, T1 arg1, T2 arg2)
		{
			return (arg3, arg4) => func (arg1, arg2, arg3, arg4);
		}

		public static Func<T4, TRes> Partial<T1, T2, T3, T4, TRes> (
			this Func<T1, T2, T3, T4, TRes> func, T1 arg1, T2 arg2, T3 arg3)
		{
			return arg4 => func (arg1, arg2, arg3, arg4);
		}

		public static Func<TRes> Partial<T1, T2, T3, T4, TRes> (
			this Func<T1, T2, T3, T4, TRes> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			return () => func (arg1, arg2, arg3, arg4);
		}

		public static Func<T1, TRes> Compose<T1, T2, TRes> (this Func<T1, T2> func1, Func<T2, TRes> func2)
		{
			return arg1 => func2 (func1 (arg1));
		}

		public static Func<T1, T2, TRes> Compose2<T1, T2, T3, TRes> (this Func<T1, T2, T3> func1, Func<T3, TRes> func2)
		{
			return (arg1, arg2) => func2 (func1 (arg1, arg2));
		}

		public static Func<T1, Func<T2, TRes>> Curry<T1, T2, TRes> (this Func<T1, T2, TRes> func)
		{
			return arg1 => (arg2 => func (arg1, arg2));
		}

		public static Func<T1, Func<T2, Func<T3, TRes>>> Curry<T1, T2, T3, TRes> (this Func<T1, T2, T3, TRes> func)
		{
			return arg1 => (arg2 => (arg3 => func (arg1, arg2, arg3)));
		}

		public static Func<T1, Func<T2, Func<T3, Func<T4, TRes>>>> Curry<T1, T2, T3, T4, TRes> (this Func<T1, T2, T3, T4, TRes> func)
		{
			return arg1 => (arg2 => (arg3 => (arg4 => func (arg1, arg2, arg3, arg4))));
		}

		public static Func<Tuple<T1, T2>, TRes> Tuplize<T1, T2, TRes> (this Func<T1, T2, TRes> func)
		{
			return t => func (t.Item1, t.Item2);
		}

		public static Func<Tuple<T1, T2, T3>, TRes> Tuplize<T1, T2, T3, TRes> (this Func<T1, T2, T3, TRes> func)
		{
			return t => func (t.Item1, t.Item2, t.Item3);
		}

		public static Func<Tuple<T1, T2, T3, T4>, TRes> Tuplize<T1, T2, T3, T4, TRes> (this Func<T1, T2, T3, T4, TRes> func)
		{
			return t => func (t.Item1, t.Item2, t.Item3, t.Item4);
		}

		public static T Identity<T> (T arg)
		{
			return arg;
		}

		public static void Ignore<T> (T value)
		{	
		}
		
		public static T Memoize<T> (Func<T> func, ref T store) where T : class
		{
			if (store == null)
				store = func ();
			return store;
		}
	}
}
