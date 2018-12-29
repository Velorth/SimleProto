using System.Text;

namespace SimpleProto.Expressions
{
    public static class EvaluationUtils
    {
        public static string InterpolateString(string str)
        {
            var result = new StringBuilder();
            var leftIndex = 0;
            var expression = new EvaluableExpression();
            var lastIndex = 0;
            while ((leftIndex = str.IndexOf('{', leftIndex)) != -1)
            {
                if (leftIndex != lastIndex)
                    result.Append(str.Substring(lastIndex, leftIndex - lastIndex));

                var rightIndex = str.IndexOf('}', leftIndex);
                if (rightIndex == -1)
                {
                    result.Append("<error>");
                    return result.ToString();
                }

                expression.ExpressionString = str.Substring(leftIndex + 1, rightIndex - leftIndex - 1);

                result.Append(expression.IsError ? "<error>" : expression.Evaluate(null));

                lastIndex = rightIndex + 1;
                leftIndex = lastIndex;
            }
            if (lastIndex < str.Length)
                result.Append(str.Substring(lastIndex));

            return result.ToString();
        }
    }
}