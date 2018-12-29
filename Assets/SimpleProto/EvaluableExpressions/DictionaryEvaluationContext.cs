using System.Collections.Generic;

namespace SimpleProto.Expressions
{
    /// <summary>
    /// Simple evaluation context.
    /// </summary>
    public sealed class DictionaryEvaluationContext : IEvaluationContext
    {
        readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        public object GetField(string field)
        {
            object result;
            if (_data.TryGetValue(field, out result))
                return result;

            return null;
        }

        public void SetField(string field, object value)
        {
            _data[field] = value;
        }
    }
}
