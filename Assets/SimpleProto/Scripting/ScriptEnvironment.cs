using System.Collections.Generic;

namespace SimpleProto.Scripting
{
    public class ScriptEnvironment
    {
        private readonly Stack<ValueContainer> _stack = new Stack<ValueContainer>();

        public bool HasValue => _stack.Count > 0;

        public void Clear() => _stack.Clear();

        public void Push(int value) => _stack.Push(new ValueContainer {IntValue = value});

        public void Push(string value) => _stack.Push(new ValueContainer { StringValue = value });

        public void Push(float value) => _stack.Push(new ValueContainer { FloatValue = value });

        public void Push(bool value) => _stack.Push(new ValueContainer { BooleanValue = value });

        public void Push(UnityEngine.Object value) => _stack.Push(new ValueContainer { ObjectValue = value });
        
        public int PopInt32() => _stack.Pop().IntValue;

        public T PopObject<T>() where T : UnityEngine.Object => (T)_stack.Pop().ObjectValue;

        public bool PopBoolean() => _stack.Pop().BooleanValue;
    }
}