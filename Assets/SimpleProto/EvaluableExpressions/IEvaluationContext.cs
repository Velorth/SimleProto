using System;

namespace SimpleProto.Expressions
{
    /// <summary>
    /// Evaluation context can be used to process external data to expression.
    /// </summary>
    public interface IEvaluationContext
    {
        object GetField(string field);
    }
}