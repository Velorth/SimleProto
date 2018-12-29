namespace SimpleProto.Expressions
{
    public enum Associativity
    {
        LeftRight,
        RightLeft
    }

    public enum ExpressionType
    {
        Void,
        Number,
        String,
        Boolean,
        Date,
        Range
    }
}