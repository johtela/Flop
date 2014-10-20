namespace Flop.Parsing
{
	using Flop;

	public class Consumed<T, S>
	{
		private Lazy<Reply<T, S>> _reply;

		public Consumed (Lazy<Reply<T, S>> reply)
		{
			_reply = reply;
		}

		public virtual bool IsEmpty
		{ 
			get { return false; }
		}

		public Reply<T, S> Reply
		{ 
			get { return _reply;}
		}
	}

	public class Empty<T, S> : Consumed<T, S>
	{
		public Empty (Lazy<Reply<T, S>> reply) : base (reply)
		{
			// Evaluate the lazy value.
			Fun.Ignore (Reply);
		}

		public override bool IsEmpty
		{
			get { return true; }
		}
	}
}