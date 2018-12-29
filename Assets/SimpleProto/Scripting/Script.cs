using System;
using JetBrains.Annotations;
using UnityEngine;

namespace SimpleProto.Scripting
{
    [Serializable]
    public sealed class Script
    {
        [SerializeField] private ScriptBlock[] _blocks = { };
        
        public object Evaluate()
        {
            var stackIndex = 0;
            object result = null;
            while (stackIndex < _blocks.Length)
            {
                result = Evaluate(ref stackIndex);
            }

            return result;
        }

        private object Evaluate(ref int index)
        {
            var block = _blocks[index];
            index++;
            switch (block.Type)
            {
                case ScriptBlockType.Object:
                    return block.ObjectValue;
                case ScriptBlockType.Integer:
                    return block.IntegerValue;
                case ScriptBlockType.Float:
                    return block.FloatValue;
                case ScriptBlockType.String:
                    return block.StringValue;
                case ScriptBlockType.Function:
                    var function = ScriptLibrary.FindFunction(block.FunctionName);
                    if (function == null)
                    {
                        Debug.LogErrorFormat("Function '{0}' not found", block.FunctionName);
                        return null;
                    }

                    var arguments = new object[block.Arity];
                    for (int argumentIndex = 0; argumentIndex < block.Arity; ++argumentIndex)
                    {
                        arguments[argumentIndex] = Evaluate(ref index);
                    }

                   return function.Evaluate(arguments);
                default:
                    throw new NotImplementedException();
            }
        }

        static Script()
        {
            // TODO: Register your scripting API here
        }
    }
}