using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleProto.Expressions
{
    public static class ExpressionLibrary
    {
        private static readonly List<OperatorInfo> Operators = new List<OperatorInfo>();
        private static readonly IDictionary<string, FunctionInfo> Functions = new Dictionary<string, FunctionInfo>();

        public static OperatorInfo FindOperator(string tokenText)
        {
            tokenText = tokenText.ToLowerInvariant();
            return Operators.FirstOrDefault(item => item.TokenText == tokenText);
        }

        public static FunctionInfo FindFunction(string tokenText)
        {
            tokenText = tokenText.ToLowerInvariant();
            FunctionInfo result = null;
            Functions.TryGetValue(tokenText, out result);

            return result;
        }

        static ExpressionLibrary ()
        {
            LoadLibrary();
        }

        static void LoadLibrary()
        {
            var loadedAssemblies = new[] {typeof (ExpressionLibrary).Assembly};

            foreach (var assembly in loadedAssemblies)
            {
                var types = assembly.GetExportedTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    foreach (var method in methods)
                    {
                        TryRegisterOperator(method);
                        TryRegisterFunction(method);
                    }
                }
            }
        }

        private static void TryRegisterOperator(MethodInfo method)
        {
            var attr = method.GetCustomAttributes(true).OfType<ExportOperatorAttribute>().FirstOrDefault();
            if (attr == null) return;

            var operatorInfo = new OperatorInfo
            {
                TokenText = attr.TokenText,
                Precedence = attr.Precedence,
                Associativity = attr.Associativity,
                Arity = attr.Arity,
                Delegate = (EvaluationDelegate) Delegate.CreateDelegate(typeof (EvaluationDelegate), method)
            };

            Operators.Add(operatorInfo);
        }

        private static void TryRegisterFunction(MethodInfo method)
        {
            var attr = method.GetCustomAttributes(true).OfType<ExportFunctionAttribute>().FirstOrDefault();
            if (attr == null) return;

            var functionInfo = new FunctionInfo
            {
                TokenText = attr.TokenText,
                Delegate = (EvaluationDelegate)Delegate.CreateDelegate(typeof(EvaluationDelegate), method)
            };

            Functions.Add(functionInfo.TokenText.ToLowerInvariant(), functionInfo);
        }
    }
}