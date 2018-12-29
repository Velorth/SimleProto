using System;
using System.Reflection;
using JetBrains.Annotations;

namespace SimpleProto.Scripting
{
    public delegate object ScriptFunction(object[] parameters);

    public sealed class FunctionInfo
    {
        public string Name { get; internal set; }

        public Type ReturnType { get; internal set; }

        public Type[] ArgTypes { get; internal set; }

        public int Arity
        {
            get { return ArgTypes.Length; }
        }

        public ScriptFunction Function { get; internal set; }

        public object Evaluate(object[] arguments)
        {
            return Function(arguments);
        }
    }
}