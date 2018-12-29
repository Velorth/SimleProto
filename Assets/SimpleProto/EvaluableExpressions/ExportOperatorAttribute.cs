using System;

namespace SimpleProto.Expressions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExportOperatorAttribute : Attribute
    {
        private int _arity = 2;
        public string TokenText { get; set; }
        public int Precedence { get; set; }
        public Associativity Associativity { get; set; }

        public int Arity
        {
            get { return _arity; }
            set { _arity = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ExportFunctionAttribute : Attribute
    {
        public string TokenText { get; set; }
    }
}
