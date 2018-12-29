namespace SimpleProto.Expressions
{
    /// <summary>
    /// </summary>
    /// <remarks>Works only with properties</remarks>
    /// TODO: Implement getting field values
    public sealed class ReflectionContext : IEvaluationContext
    {
        public ReflectionContext(object source)
        {
            Source = source;
        }

        public object Source { get; private set; }

        public object GetField(string field)
        {
            if (string.IsNullOrEmpty(field) || Source == null)
            {
                return null;
            }
            
            var properties = field.Split('.');

            var currentObject = Source;

            foreach (var propertyName in properties)
            {
                var currentType = currentObject.GetType();
                var property = currentType.GetProperty(propertyName);
                if (property == null)
                {
                    currentObject = null;
                    break;
                }
                currentObject = property.GetValue(currentObject, null);
                if (currentObject == null)
                {
                    break;
                }
            }

            return currentObject;
        }
    }
}
