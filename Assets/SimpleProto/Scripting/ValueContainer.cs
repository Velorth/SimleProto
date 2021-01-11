using System.Runtime.InteropServices;
using Object = UnityEngine.Object;

namespace SimpleProto.Scripting
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct ValueContainer
    {
        [field: FieldOffset(0)]
        public bool BooleanValue { get; set; }

        [field:FieldOffset(0)]
        public int IntValue { get; set; }

        [field:FieldOffset(0)]
        public float FloatValue { get; set; }

        [field:FieldOffset(0)]
        public string StringValue { get; set; }

        [field:FieldOffset(0)]
        public Object ObjectValue { get; set; }
    }
}