namespace CodeBetter.Json
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Helpers;

    public delegate object PreFieldSerializingDelegate(string name, object value);
    
    public class JsonSerializer
    {
        //private readonly StringBuilder _builder;
        private readonly JsonWriter _writer;
        private readonly string _fieldPrefix;
        private ArrayList _currentGraph;
        private readonly PreFieldSerializingDelegate _callback;

        public JsonSerializer(JsonWriter writer, PreFieldSerializingDelegate callback) : this(writer, string.Empty, callback)
        {
        }
        public JsonSerializer(JsonWriter writer, string fieldPrefix, PreFieldSerializingDelegate callback)
        {
            _writer = writer;
            _currentGraph = new ArrayList(0);
            _fieldPrefix = fieldPrefix;
            _callback = callback;
        }
        
        public static void Serialize(JsonWriter writer, object instance)
        {
            Serialize(writer, instance, string.Empty);
        }
        public static void Serialize(JsonWriter writer, object instance, string fieldPrefix)
        {
            Serialize(writer, instance, fieldPrefix, null);            
        }
        public static void Serialize(JsonWriter writer, object instance, PreFieldSerializingDelegate callback)
        {
            Serialize(writer, instance, string.Empty, callback);
        }
        public static void Serialize(JsonWriter writer, object instance, string fieldPrefix, PreFieldSerializingDelegate callback)
        {
            new JsonSerializer(writer, fieldPrefix, callback).SerializeValue("root", instance);
        }
        
        private void SerializeValue(string name, object value)
        {
            if (_callback != null)
            {
                value = _callback(name, value);
            }
            if (value == null)
            {
                _writer.WriteNull();
                return;
            }
            if (value is string)
            {
                _writer.WriteString((string) value);
                return;
            }
            if (value is int || value is long || value is short || value is float || value is byte || value is sbyte || value is uint || value is ulong || value is ushort || value is double)
            {
                _writer.WriteRaw(value.ToString());               
                return;
            }
            if (value is char)
            {
                _writer.WriteChar((char) value);                
                return;
            }
            if (value is bool)
            {
                _writer.WriteBool((bool)value);                
                return;
            }
            if (value is DateTime)
            {
                _writer.WriteDate((DateTime) value);                
                return;
            }
            if (value is IDictionary)
            {
                SerializeDictionary((IDictionary) value);
                return;
            }
            if (value is IEnumerable)
            {
                SerializeEnumerable((IEnumerable) value);
                return;
            }
            SerializeObject(value);
        }

        private void SerializeObject(object @object)
        {
            if (@object == null)
            {
                return;
            }
            var fields = ReflectionHelper.GetSerializableFields(@object.GetType());
            if (fields.Count == 0)
            {
                return;
            }
            if (_currentGraph.Contains(@object))
            {
                throw new JsonException("Recursive reference found. Serialization cannot complete. Consider marking the offending field with the NonSerializedAttribute");
            }

            var oldGraph = _currentGraph;
            var currentGraph = new ArrayList(_currentGraph);
            _currentGraph = currentGraph;
            _currentGraph.Add(@object);

            _writer.BeginObject();
            SerializeKeyValue(GetKeyName(fields[0]), ReflectionHelper.GetValue(fields[0], @object), true);
            for (var i = 1; i < fields.Count; ++i)
            {
                SerializeKeyValue(GetKeyName(fields[i]), ReflectionHelper.GetValue(fields[i], @object), false);
            }
            _writer.EndObject();
            _currentGraph = oldGraph;
        }

        private string GetKeyName(MemberInfo field)
        {
            var name = field.Name;
            return name.StartsWith(_fieldPrefix) ? name.Substring(_fieldPrefix.Length) : name;
        }

        private void SerializeEnumerable(IEnumerable value)
        {
            var enumerator = value.GetEnumerator();
            _writer.BeginArray();
            var index = 0;        
            if (enumerator.MoveNext())
            {
                SerializeValue((index++).ToString(), enumerator.Current);
            }
            while (enumerator.MoveNext())
            {
                _writer.SeparateElements();
                SerializeValue((index++).ToString(), enumerator.Current);
            }            
            _writer.EndArray();
        }

        private void SerializeDictionary(IDictionary value)
        {
            var enumerator = value.GetEnumerator();
            _writer.BeginObject();            
            if (enumerator.MoveNext())
            {
                SerializeKeyValue(enumerator.Key.ToString(), enumerator.Value, true);
            }
            while (enumerator.MoveNext())
            {
                SerializeKeyValue(enumerator.Key.ToString(), enumerator.Value, false);
            }            
            _writer.EndObject();
        }

        private void SerializeKeyValue(string key, object value, bool isFirst)
        {
            if (!isFirst)
            {
                _writer.SeparateElements();
                _writer.NewLine();
            }
            _writer.WriteKey(key);
            SerializeValue(key, value);
        }
    }
}