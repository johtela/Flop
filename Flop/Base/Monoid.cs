namespace Flop
{
	using System;

	public interface IMonoid<T> where T : new () 
	{
		T Plus (T other);
	}

	public interface IMeasurable<V> where V: IMonoid<V>, new ()
	{
		V Measure ();
	}
}
