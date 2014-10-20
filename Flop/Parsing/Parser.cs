namespace Flop.Parsing
{
	using System;
	using System.Text;
	using Flop.Collections;

	/// <summary>
	/// A parser is a function that reads input from a sequence and returns a 
	/// parsed value. The types of the input stream and the parsed value are 
	/// generic, so effectively parser can read any stream and return any value.
	/// </summary>
	public delegate Consumed<T, S> Parser<T, S> (IInput<S> input);

	/// <summary>
	/// Monadic parsing operations implemented as extensions methods for the
	/// Parser[T, S] delegate.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// Attempt to parse an input with a given parser.
		/// </summary>
		public static Either<T, ParseError> TryParse<T, S>(this Parser<T, S> parser, IInput<S> input)
		{
			var res = parser (input);
			return res.Reply ?
				Either<T, ParseError>.Create (res.Reply.Result) : 
				Either<T, ParseError>.Create (ParseError.FromReply (res.Reply));
		}

		/// <summary>
		/// Parses an input, or throws an ParseError exception, if the parse fails.
		/// </summary>
		public static T Parse<T, S> (this Parser<T, S> parser, IInput<S> input)
		{
			return TryParse (parser, input).Match (
				value => value,
				error => { throw error; });
		}

		/// <summary>
		/// The monadic bind. Runs the first parser, and if it succeeds, feeds the
		/// result to the second parser. Corresponds to Haskell's >>= operator.
		/// </summary>
		public static Parser<U, S> Bind<T, U, S> (this Parser<T, S> parser, Func<T, Parser<U, S>> func)
		{
			return input =>
			{
				var res1 = parser (input);
				if (res1.IsEmpty)
				{
					if (res1.Reply)
					{
						var res2 = func (res1.Reply.Result) (res1.Reply.Input);
						return res2.IsEmpty ?
							new Empty<U, S> (Lazy.Create (res2.Reply.MergeExpected (res1.Reply))) :
							res2;
					}
					return new Empty<U, S> (
						Lazy.Create (Reply<U, S>.Fail (res1.Reply.Input, res1.Reply.Found, res1.Reply.Expected)));
				} 
				else
					return new Consumed<U, S> (Lazy.Create (() =>
					{
						if (res1.Reply)
						{
							var res2 = func (res1.Reply.Result) (res1.Reply.Input);
							return res2.IsEmpty ?
							 		res2.Reply.MergeExpected (res1.Reply) :
									res2.Reply;
						}
						return Reply<U, S>.Fail (res1.Reply.Input, res1.Reply.Found, res1.Reply.Expected);
					}));
			};
		}

		/// <summary>
		/// The monadic sequencing. Runs the first parser, and if it succeeds, runs the second
		/// parser ignoring the result of the first one. Corresponds to Haskell's >> operator.
		/// </summary>
		public static Parser<U, S> Seq<T, U, S> (this Parser<T, S> parser, Parser<U, S> other)
		{
			return parser.Bind (_ => other);
		}

		/// <summary>
		/// The monadic return. Lifts a value to the parser monad, i.e. creates
		/// a parser that just returns a value without consuming any input.
		/// </summary>
		public static Parser<T, S> ToParser<T, S> (this T value)
		{
			return input => new Empty<T, S> (Lazy.Create (Reply<T, S>.Ok (value, input)));
		}

		public static Parser<T, S> Fail<T, S> (string found, string expected)
		{ 
			return input => new Empty<T, S> (Lazy.Create (Reply<T, S>.Fail (
				input, found, string.IsNullOrEmpty(expected) ? 
					LazyList<string>.Empty :
					LazyList.Create (expected))));
		}

		/// <summary>
		/// Creates a parser that reads one item from input and returns it, if
		/// it satisfies a given predicate; otherwise the parser will fail.
		/// </summary>
		public static Parser<T, T> Satisfy<T> (Func<T, bool> predicate)
		{
			return input =>
			{
				if (input.IsEmpty)
					return new Empty<T, T> (Lazy.Create (Reply<T, T>.Fail (input, "end of input")));
				var item = input.First;
				return predicate (item) ?
					new Consumed<T, T> (Lazy.Create (Reply<T, T>.Ok (item, input.Rest))) :
					new Empty<T, T> (Lazy.Create (Reply<T, T>.Fail (input, item.ToString ())));
			};
		}

		/// <summary>
		/// The monadic plus operation. Creates a parser that runs the first parser, and if
		/// that fails, runs the second one. Corresponds to the | operation in BNF grammars.
		/// </summary>
		public static Parser<T, S> Plus<T, S> (this Parser<T, S> parser, Parser<T, S> other)
		{
			return input =>
			{
				var res1 = parser (input);
				if (res1.IsEmpty && !res1.Reply)
				{
					var res2 = other (input);
					return res2.IsEmpty ?
						new Empty<T, S> (Lazy.Create (res2.Reply.MergeExpected (res1.Reply))) :
						res2;
				}
				return res1;
			};
		}

		public static Parser<T, S> Label<T, S> (this Parser<T, S> parser, string expected)
		{
			return input =>
			{
				var res = parser (input);
				if (res.IsEmpty)
				{
					var exp = LazyList.Create (expected);
					return new Empty<T, S> (Lazy.Create (res.Reply ?
						Reply<T, S>.Ok (res.Reply.Result, res.Reply.Input, res.Reply.Found, exp) :
						Reply<T, S>.Fail (res.Reply.Input, res.Reply.Found, exp)));
				}
				return res;
			};
		}

		/// <summary>
		/// Select extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Parser<U, S> Select<T, U, S> (this Parser<T, S> parser, Func<T, U> select)
		{
			return parser.Bind (x => select (x).ToParser<U, S> ());
		}

		/// <summary>
		/// SelectMany extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Parser<V, S> SelectMany<T, U, V, S> (this Parser<T, S> parser,
			Func<T, Parser<U, S>> project, Func<T, U, V> select)
		{
			return parser.Bind (x => project (x).Bind (y => select (x, y).ToParser<V, S> ()));
		}

		/// <summary>
		/// Creates a parser that will run a given parser zero or more times. The results
		/// of the input parser are added to a list.
		/// </summary>
		public static Parser<StrictList<T>, S> Many<T, S> (this Parser<T, S> parser)
		{
			return (from x in parser
					from xs in parser.Many ()
					select x | xs)
					.Plus (StrictList<T>.Empty.ToParser<StrictList<T>, S> ());
		}

		/// <summary>
		/// Creates a parser that will run a given parser one or more times. The results
		/// of the input parser are added to a list.
		/// </summary>
		public static Parser<StrictList<T>, S> Many1<T, S> (this Parser<T, S> parser)
		{
			return from x in parser
				   from xs in parser.Many ()
				   select x | xs;
		}

		/// <summary>
		/// Optionally parses an input.
		/// </summary>
		public static Parser<Option<T>, S> Optional<T, S> (this Parser<T, S> parser)
		{
			return parser.Bind (x => new Option<T> (x).ToParser<Option<T>, S> ())
				.Plus (new Option<T> ().ToParser<Option<T>, S> ());
		}

		/// <summary>
		/// Optionally parses an input, if the parser fails then the default value is returned.
		/// </summary>
		public static Parser<T, S> Optional<T, S> (this Parser<T, S> parser, T defaultValue)
		{
			return parser.Plus (defaultValue.ToParser<T, S> ());
		}

		/// <summary>
		/// Creates a parser that will read a list of items separated by a separator.
		/// The list needs to have at least one item.
		/// </summary>
		public static Parser<StrictList<T>, S> SeparatedBy1<T, U, S> (this Parser<T, S> parser,
			Parser<U, S> separator)
		{
			return from x in parser
				   from xs in
					   (from y in separator.Seq (parser)
						select y).Many ()
				   select x | xs;
		}

		/// <summary>
		/// Creates a parser that will read a list of items separated by a separator.
		/// The list can also be empty.
		/// </summary>
		public static Parser<StrictList<T>, S> SeparatedBy<T, U, S> (this Parser<T, S> parser,
			Parser<U, S> separator)
		{
			return SeparatedBy1 (parser, separator).Plus (
				StrictList<T>.Empty.ToParser<StrictList<T>, S> ());
		}

		/// <summary>
		/// Creates a parser the reads a bracketed input.
		/// </summary>
		public static Parser<T, S> Bracket<T, U, V, S> (this Parser<T, S> parser,
			Parser<U, S> open, Parser<V, S> close)
		{
			return from o in open
				   from x in parser
				   from c in close
				   select x;
		}

		/// <summary>
		/// Creates a parser that reads an expression with multiple terms separated
		/// by an operator. The operator is returned as a function and the terms are
		/// evaluated left to right.
		/// </summary>
		public static Parser<T, S> ChainLeft1<T, S> (this Parser<T, S> parser,
			Parser<Func<T, T, T>, S> operation)
		{
			return from x in parser
				   from fys in
					   (from f in operation
						from y in parser
						select new { f, y }).Many ()
				   select fys.ReduceLeft (x, (z, fy) => fy.f (z, fy.y));
		}

		/// <summary>
		/// Creates a parser that reads an expression with multiple terms separated
		/// by an operator. The operator is returned as a function and the terms are
		/// evaluated right to left.
		/// </summary>
		public static Parser<T, S> ChainRight1<T, S> (this Parser<T, S> parser,
			Parser<Func<T, T, T>, S> operation)
		{
			return parser.Bind (x =>
				   (from f in operation
					from y in ChainRight1 (parser, operation)
					select f (x, y))
					.Plus (x.ToParser<T, S> ())
			);
		}

		/// <summary>
		/// Creates a parser that reads an expression with multiple terms separated
		/// by an operator. The operator is returned as a function and the terms are
		/// evaluated left to right. If the parsing of the expression fails, the value
		/// given as an argument is returned as a parser.
		/// </summary>
		public static Parser<T, S> ChainLeft<T, S> (this Parser<T, S> parser,
			Parser<Func<T, T, T>, S> operation, T value)
		{
			return parser.ChainLeft1 (operation).Plus (value.ToParser<T, S> ());
		}

		/// <summary>
		/// Creates a parser that reads an expression with multiple terms separated
		/// by an operator. The operator is returned as a function and the terms are
		/// evaluated right to left. If the parsing of the expression fails, the value
		/// given as an argument is returned as a parser.
		/// </summary>
		public static Parser<T, S> ChainRight<T, S> (this Parser<T, S> parser,
			Parser<Func<T, T, T>, S> operation, T value)
		{
			return parser.ChainRight1 (operation).Plus (value.ToParser<T, S> ());
		}

		/// <summary>
		/// Create a combined parser that will parse any of the given operators. 
		/// The operators are specified in a seqeunce which contains (parser, result)
		/// pairs. If the parser succeeds the result is returned, otherwise the next 
		/// parser in the sequence is tried.
		/// </summary>
		public static Parser<U, S> Operators<T, U, S> (ISequence<Tuple<Parser<T, S>, U>> ops)
		{
			return ops.Map (op => from _ in op.Item1
								  select op.Item2).ReduceLeft1 (Plus);
		}

		/// <summary>
		/// Upcast the result of the parser.
		/// </summary>
		public static Parser<U, S> Cast<T, U, S> (this Parser<T, S> parser) where T : U
		{
			return from x in parser
				   select (U)x;
		}
	}
}
