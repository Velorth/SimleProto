using System;

namespace SimpleProto.Expressions
{
    public class ParsingException : Exception
    {
        public ParsingException(string message = "") : base(message)
        {
            
        }
    }
}
