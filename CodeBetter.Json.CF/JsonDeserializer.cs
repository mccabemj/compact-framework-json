namespace CodeBetter.Json
{
    using System;
    using System.Collections;
    using System.Collections.Generic;    
    
    using System.Reflection;    
    using Helpers;

    public class JsonDeserializer
    {
        private static readonly Type _IEnumerableType = typeof(IEnumerable);
        private readonly JsonReader _reader;
        private readonly string _fieldPrefix;

        public JsonDeserializer(JsonReader reader) : this(reader, string.Empty){}
        public JsonDeserializer(JsonReader reader, string fieldPrefix)
        {
            _reader = reader;
            _fieldPrefix = fieldPrefix;
        }

        public static T Deserialize<T>(JsonReader reader)
        {
            return Deserialize<T>(reader, string.Empty);
        }
        public static T Deserialize<T>(JsonReader reader, string fieldPrefix)
        {
            return (T) new JsonDeserializer(reader, fieldPrefix).DeserializeValue(typeof(T));
        }

        

        private object DeserializeValue(Type type)
        {
            _reader.SkipWhiteSpaces();
            var isNullable = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;
                type = Nullable.GetUnderlyingType(type);
            }       
            if (type == typeof(int))
            {
                return _reader.ReadInt32(isNullable);                
            }
            if (type == typeof(string))
            {
                return _reader.ReadString();                
            }
            if (type == typeof(double))
            {
                return _reader.ReadDouble(isNullable);
            }
            if (type == typeof(DateTime))
            {
                return _reader.ReadDateTime(isNullable);
            }
            if (_IEnumerableType.IsAssignableFrom(type))
            {
                return DeserializeList(type);
            }            
            if (type == typeof(char))
            {
                return _reader.ReadChar();
            }
            if (type == typeof(bool))
            {
                return _reader.ReadBool(isNullable);
            }
            if (type.IsEnum)
            {
                return _reader.ReadEnum(type);
            }
            if (type == typeof(long))
            {
                return _reader.ReadInt64(isNullable);
            }        
            if (type == typeof(float))
            {
                return _reader.ReadFloat(isNullable);
            }
            if (type == typeof(short))
            {
                return _reader.ReadInt16(isNullable);
            }            
            return ParseObject(type);            
        }
        private object DeserializeList(Type listType)
        {
            _reader.SkipWhiteSpaces();
            _reader.AssertAndConsume(JsonTokens.StartArrayCharacter);            
            var itemType = ListHelper.GetListItemType(listType);
            bool isReadonly;
            var container = ListHelper.CreateContainer(listType, itemType, out isReadonly);
            while(true)
            {
                _reader.SkipWhiteSpaces();
                container.Add(DeserializeValue(itemType));
                _reader.SkipWhiteSpaces();                
                if (_reader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndArrayCharacter))
                {
                    break;
                }                
            }
            if (listType.IsArray)
            {
                return ListHelper.ToArray((List<object>)container, itemType);
            }
            if (isReadonly)
            {
                return listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { container.GetType() }, null).Invoke(new object[] { container });
            }
            return container;
        }
        private object ParseObject(Type type)
        {           
            _reader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            var constructor = ReflectionHelper.GetDefaultConstructor(type);
            var instance = constructor.Invoke(null);
            while (true)
            {
                _reader.SkipWhiteSpaces();
                var name = _reader.ReadString();
                if (!name.StartsWith(_fieldPrefix))
                {
                    name = _fieldPrefix + name;
                }
                var field = ReflectionHelper.FindField(type, name);
                _reader.SkipWhiteSpaces();
                _reader.AssertAndConsume(JsonTokens.PairSeparator);                
                _reader.SkipWhiteSpaces();
                field.SetValue(instance, DeserializeValue(field.FieldType));
                _reader.SkipWhiteSpaces();
                if (_reader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                } 
            }
            return instance;
        }
    }
}