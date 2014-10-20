namespace Flop.Collections
{
	using System.Collections.Generic;

	/// <summary>
	/// Interface for building various stream types.
	/// </summary>
	/// <typeparam name="S">Stream type.</typeparam>
	/// <typeparam name="T">The stream's item type.</typeparam>
	public interface IStreamBuilder<S, T> where S : IStream<T>
	{
		/// <summary>
		/// Return an empty stream.
		/// </summary>
		S Empty { get; }

		/// <summary>
		/// The classical const or push operation adds an item to the front of the stream.
		/// </summary>
		S Cons (T first, S rest);

		/// <summary>
		/// Constructs a stream from IEnumerable.
		/// </summary>
		S FromEnumerable (IEnumerable<T> items);
	}
}