using System;
using System.Globalization;
using System.Linq;

namespace SimpleProto.Expressions
{
    public static class StandardLibrary
    {
        [ExportOperator(Precedence = 3, TokenText = "~-", Arity = 1, Associativity = Associativity.RightLeft)]
        public static object UnaryMinus(IEvaluationContext context, IEvaluable[] parameters)
        {
            var first = parameters[0].Evaluate(context);
            var value = Convert.ToSingle(first, CultureInfo.InvariantCulture);
            return -value;
        }

        [ExportOperator(Precedence = 9, TokenText = "=")]
        public static object Equals(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);
            return Equals(left, right);
        }

        [ExportOperator(Precedence = 9, TokenText = "<>")]
        public static object NonEquals(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);
            return !Equals(left, right);
        }

        [ExportOperator(Precedence = 8, TokenText = ">")]
        public static object Greater(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return CompareEx(left, right) == 1;
        }

        [ExportOperator(Precedence = 8, TokenText = "<")]
        public static object Less(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return CompareEx(left, right) == -1;
        }

        [ExportOperator(Precedence = 8, TokenText = ">=")]
        public static object GreaterOrEquals(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return CompareEx(left, right) > -1;
        }

        [ExportOperator(Precedence = 8, TokenText = "<=")]
        public static object LessOrEquals(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return CompareEx(left, right) < 1;
        }

        [ExportOperator(Precedence = 6, TokenText = "+")]
        public static object Add(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            if (left is string)
                return string.Concat(left, right);
            
            return Convert.ToSingle(left) + Convert.ToSingle(right);
        }

        [ExportOperator(Precedence = 6, TokenText = "-")]
        public static object Sub(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return Convert.ToSingle(left) - Convert.ToSingle(right);
        }

        [ExportOperator(Precedence = 5, TokenText = "*")]
        public static object Mult(IEvaluationContext context, IEvaluable[] parameters)
        {
            var left = parameters[0].Evaluate(context);
            var right = parameters[1].Evaluate(context);

            return Convert.ToSingle(left) * Convert.ToSingle(right);
        }

        [ExportOperator(Precedence = 13, TokenText = "and")]
        public static object And(IEvaluationContext context, IEvaluable[] parameters)
        {
            return parameters.All(node => Convert.ToBoolean(node.Evaluate(context)));
        }

        [ExportOperator(Precedence = 14, TokenText = "or")]
        public static object Or(IEvaluationContext context, IEvaluable[] parameters)
        {
            return parameters.Any(node => Convert.ToBoolean(node.Evaluate(context)));
        }

        [ExportOperator(Precedence = 15, TokenText = "?:", Associativity = Associativity.RightLeft, Arity = 3)]
        public static object Ternary(IEvaluationContext context, IEvaluable[] parameters)
        {
            var condition = Convert.ToBoolean(parameters[0].Evaluate(context));
            return condition ? parameters[1].Evaluate(context) : parameters[2].Evaluate(context);
        }

        [ExportFunction(TokenText = "Max")]
        public static object Max(IEvaluationContext context, IEvaluable[] parameters)
        {
            var max = parameters[0].Evaluate(context);
            for (var i = 1; i < parameters.Length; ++i)
            {
                var value = parameters[i].Evaluate(context);
                if (CompareEx(value, max) > 0)
                {
                    max = value;
                }
            }

            return max;
        }

        /// <summary>
        /// Compares two values using duck typing
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// 1 if a is greater than b;
        /// -1 if a is lesser than b;
        /// 0 if a is equal to b;
        /// null if comparasion is impossible;
        /// </returns>
        static int? CompareEx(object a, object b)
        {
            if (Equals(a, b))
                return 0;

            if (a == null || b == null)
                return null;

            if (IsNumericType(a) || IsNumericType(b))
            {
                var floatA = Convert.ToSingle(a);
                var floatB = Convert.ToSingle(b);
                return floatA > floatB ? 1 : -1;
            }

            if (a is DateTime && b is DateTime)
            {
                return ((DateTime) a).CompareTo((DateTime) b);
            }

            if (a is DateTime && b is string)
            {
                DateTime bDate;
                if (DateTime.TryParse((string) b, out bDate))
                    return ((DateTime) a).CompareTo(bDate);

                return null;
            }

            if (b is DateTime && a is string)
            {
                DateTime aDate;
                if (DateTime.TryParse((string)a, out aDate))
                    return aDate.CompareTo((DateTime)b);

                return null;
            }

            if (a is string && b is string)
            {
                return string.Compare((string) a, (string) b, StringComparison.InvariantCulture);
            }

            return null;
        }

        public static bool IsNumericType(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
    }
}
