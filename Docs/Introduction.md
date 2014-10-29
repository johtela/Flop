Flop - Library for Writing C# Programs in Functional Style
==========================================================

Introduction
------------

Flop is a general purpose library that helps writing C# programs in functional style. It offers various features commonly found in functional languages, such as immutable data structures, parser combinators, and property based testing. When using Flop your programs resemble more code written in F# or Haskell than idiomatic, imperative C#. The goal of the library is to make C# programs more succinct and powerful using the principles of functional programming. The benefits of functional paradigm, such as easier reasoning about program correctness and better support for concurrency, become more achievable with the help of the library.

 The documentation covers the library bottom-up, first explaining the basic concepts like immutable values, tuples, options, and of course, first-class functions and lambdas. These concepts are probably already familiar to most programmers as many of them are already implemented in the C# language or in the .NET framework.

Immutable data structures are tackled next; lists, trees, sets, maps, and so on. In practical terms, immutability means that a data structure is updated by creating new copies of it, instead of changing or mutating it. The copies share substantial parts of the original data structure, so the update operations are very fast in practice. Some data structures are also *lazy*, which is another common concept in functional languages. It means essentially that some part of a data structure is not created until it is actually used somewhere in the code.

Once the data structures are covered, we get acquainted with the various abstractions that classify them. The term abstraction, in this context, can refer to an interface, a delegate type, or a set of types and methods that together constitute some higher level concept. The most powerful, or at least the most hyped abstraction we encounter is the *monad*. One manifestation of a monad can be found in the LINQ library. LINQ also provides convenient syntactic sugar that we can make use of in our own monads.

After these concepts are understood we can delve into how parser combinators and property based testing work. Parser combinators can be used to build complex parsers from simple primitives, and property based testing allows us to verify assertions about a program with automatically generated data. These libraries are shamelessly copied from Haskell, but adapted to C# providing an API that suits more the language. The main contribution of the Flop library is that it ports these powerful tools to C# without sacrificing efficiency or elegance of the code.

There are probably differing opinions about the beauty of functional-style C# code. One could argue that, when crafted tastefully, it can be as readable and understandable as idiomatic C#. The benefits of functional style stem from working with higher-level concepts and achieving more with less code. For example, instead of using looping through a collection with a `for` statement, one can traverse it with higher level function such as `map`, `filter`, or `reduce`. By doing computation in declarative way using (pure) functions, the resulted code becomes more *composable* than traditional, imperative code. Paradoxically, the promise of code re-usability, which was a big selling point for OO languages, is finally started to realize as the functional paradigm has become more mainstream.

The disadvantage of writing C# in functional style is that it makes it harder to follow the code in the debugger. This problem stems from the fact that lot of plumbing is hidden behind lambdas and continuations. In debugger this plumbing becomes visible and is hard to skip. However, most of the time it is faster and easier to find the source of a bug by testing and reasoning about the code than it is to step through it in the debugger. The good news is that when most of the functions are pure (without side effects), it is easy to test them in isolation and pinpoint bugs by calling the functions interactively in the debugger, or by writing tests for them.


Table of Contents
-----------------

[Basic Concepts](Basic Concepts.html)

