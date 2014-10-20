using System;

namespace Flop
{
    // The general exception class.
    public class NOPException : Exception
    {
        public NOPException(string message) : base(message)
        { }
    }
}
