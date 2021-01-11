using System;

namespace SimpleProto.Scripting
{
    public delegate void ScriptFunction(ScriptEnvironment environment);

    public sealed class FunctionInfo
    {
        public string Name { get; internal set; }

        public Type ReturnType { get; internal set; }

        public Type[] ArgTypes { get; internal set; }

        public int Arity => ArgTypes.Length;

        public ScriptFunction Function { get; internal set; }
    }
}