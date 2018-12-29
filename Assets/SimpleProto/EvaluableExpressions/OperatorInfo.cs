namespace SimpleProto.Expressions
{
    public delegate object EvaluationDelegate(IEvaluationContext context, IEvaluable[] parameters);

    public sealed class OperatorInfo
    {
        public string TokenText { get; set; }
        public int Precedence { get; set; }
        public EvaluationDelegate Delegate { get; set; }
        public Associativity Associativity { get; set; }

        public int Arity { get; set; }

        public Token BuildToken()
        {
            return new Token
            {
                Text = TokenText,
                Type = TokenType.Operator,
                Precedence = Precedence,
                IsLeftAssociative = Associativity == Associativity.LeftRight
            };
        }
    }

    public sealed class FunctionInfo
    {
        public string TokenText { get; set; }
        public EvaluationDelegate Delegate { get; set; }

        public Token BuildToken()
        {
            return new Token
            {
                Text = TokenText,
                Type = TokenType.Function
            };
        }
    }
}