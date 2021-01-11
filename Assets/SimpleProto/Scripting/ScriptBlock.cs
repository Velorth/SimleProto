using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SimpleProto.Scripting
{
    public enum ScriptBlockType
    {
        Function = 0,
        Object = 1,
        Boolean = 2,
        String = 3,
        Integer = 4,
        Float = 5,
    }

    [Serializable]
    internal class ScriptBlock
    {
        [SerializeField] private ScriptBlockType _type = ScriptBlockType.Integer;
        [SerializeField] private string _functionName = "";
        [SerializeField] private Object _objectValue = null;
        [SerializeField] private string _stringValue = "";
        [SerializeField] private int _intValue = 0;
        [SerializeField] private float _floatValue = 0f;
        [SerializeField] private bool _booleanValue = false;

        public ScriptBlockType Type
        {
            get { return _type; }
        }

        public string FunctionName
        {
            get { return _functionName; }
        }

        public Object ObjectValue
        {
            get { return _objectValue; }
        }

        public bool BooleanValue
        {
            get { return _booleanValue; }
        }

        public int IntegerValue
        {
            get { return _intValue; }
        }

        public string StringValue
        {
            get { return _stringValue; }
        }

        public float FloatValue
        {
            get { return _floatValue; }
        }
    }
}
