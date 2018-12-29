using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProto.Expressions
{
    public interface IEvaluable
    {
        object Evaluate(IEvaluationContext context);
    }

    public sealed class EvaluableExpression
    {
        private ExpressionParser Parser
        {
            get { return _parser; }
        }

        private string _expressionString;
        private readonly ExpressionParser _parser = new ExpressionParser();

        public string ExpressionString
        {
            get { return _expressionString; }
            set
            {
                if (_expressionString == value) return;
                _expressionString = value;
                try
                {
                    RootNode = Parser.BuildExpression(_expressionString);
                    IsError = false;
                }
                catch (Exception)
                {
                    RootNode = null;
                    IsError = true;
                }
            }
        }

        public bool IsError { get; private set; }

        private IEvaluable RootNode { get; set; }

        public object Evaluate(IEvaluationContext context)
        {
            return RootNode != null ? RootNode.Evaluate(context) : null;
        }

        public static bool IsNullOrEmpty(EvaluableExpression expression)
        {
            return expression == null || expression.RootNode == null || string.IsNullOrEmpty(expression.ExpressionString);
        }
    }

    public sealed class ConstNode : IEvaluable
    {
        public object Value { get; set; }

        public object Evaluate(IEvaluationContext context)
        {
            return Value;
        }
    }

    public sealed class FieldNode : IEvaluable
    {
        public string FieldName { get; set; }

        public object Evaluate(IEvaluationContext context)
        {
            return context.GetField(FieldName);
        }
    }

    public sealed class OperatorNode : IEvaluable
    {
        private readonly List<IEvaluable> _children = new List<IEvaluable>();

        public OperatorInfo Operator { get; set; }

        public IList<IEvaluable> Children { get { return _children; } }

        public object Evaluate(IEvaluationContext context)
        {
            return Operator.Delegate(context, Children.ToArray());
        }
    }

    public sealed class FunctionNode : IEvaluable
    {
        private readonly List<IEvaluable> _children = new List<IEvaluable>();

        public FunctionInfo Function { get; set; }

        public IList<IEvaluable> Children { get { return _children; } }

        public object Evaluate(IEvaluationContext context)
        {
            return Function.Delegate(context, Children.ToArray());
        }
    }
}