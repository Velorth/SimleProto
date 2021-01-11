using System;
using JetBrains.Annotations;
using UnityEngine;

namespace SimpleProto.Scripting
{
    [Serializable]
    public sealed class Script
    {
        [SerializeField] private ScriptBlock[] _blocks = { };

        private readonly ScriptEnvironment _environment = new ScriptEnvironment();

        public void Evaluate()
        {
            _environment.Clear();
            var stackIndex = 0;
            while (stackIndex < _blocks.Length)
            {
                Evaluate(ref stackIndex);
            }

        }

        public bool? EvaluateBoolean()
        {
            _environment.Clear();
            var stackIndex = 0;
            while (stackIndex < _blocks.Length)
            {
                Evaluate(ref stackIndex);

                if (_environment.HasValue)
                {
                    var lastResult = _environment.PopBoolean();
                    if (!lastResult)
                        return false;
                }
            }

            return true;
        }

        private void Evaluate(ref int index)
        {
            var block = _blocks[index];
            index++;
            switch (block.Type)
            {
                case ScriptBlockType.Object:
                    _environment.Push(block.ObjectValue);
                    break;
                case ScriptBlockType.Integer:
                    _environment.Push(block.IntegerValue);
                    break;
                case ScriptBlockType.Float:
                    _environment.Push(block.FloatValue);
                    break;
                case ScriptBlockType.String:
                    _environment.Push(block.StringValue);
                    break;
                case ScriptBlockType.Function:
                    var function = ScriptLibrary.FindFunction(block.FunctionName);
                    if (function == null)
                    {
                        throw new NotSupportedException($"Function '{block.FunctionName}' not found");
                    }

                    for (int argumentIndex = 0; argumentIndex < function.Arity; ++argumentIndex)
                    {
                        Evaluate(ref index);
                    }

                    function.Function(_environment);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        static Script()
        {
            ScriptLibrary.RegisterFunction(new FunctionInfo { ArgTypes = new []{ typeof(bool) }, ReturnType = typeof(bool), Name = "not", Function = OperatorNot });
        }
        

        public static void OperatorNot(ScriptEnvironment e)
        {
            var arg = e.PopBoolean();
            e.Push(!arg);
        }
    }
}