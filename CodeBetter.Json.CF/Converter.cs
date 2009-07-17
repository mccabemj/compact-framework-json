namespace CodeBetter.Json
{
    using System.Globalization;
    using System.IO;
    
    
    public class Converter
    {
        public static void Serialize(Stream output, object instance)
        {
            Serialize(output, instance, string.Empty);
        }
        public static void Serialize(Stream output, object instance, string fieldPrefix)
        {
            var writer = new JsonWriter(output);
            JsonSerializer.Serialize(writer, instance, fieldPrefix);
            writer.Flush();
        }
        public static void Serialize(string file, object instance)
        {
            Serialize(file, instance, string.Empty);
        }
        public static void Serialize(string file, object instance, string fieldPrefix)
        {
            using (var writer = new JsonWriter(file))
            {
                JsonSerializer.Serialize(writer, instance, fieldPrefix);                
            }              
        }
        public static string Serialize(object instance)
        {
            return Serialize(instance, string.Empty);
        }      
        public static string Serialize(object instance, string fieldPrefix)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new JsonWriter(sw))
            {
                JsonSerializer.Serialize(writer, instance, fieldPrefix);
                return sw.ToString();
            }               
        }

        public static void Serialize(Stream output, object instance, PreFieldSerializingDelegate callback)
        {
            Serialize(output, instance, string.Empty);
        }
        public static void Serialize(Stream output, object instance, string fieldPrefix, PreFieldSerializingDelegate callback)
        {
            var writer = new JsonWriter(output);
            JsonSerializer.Serialize(writer, instance, fieldPrefix, callback);
            writer.Flush();
        }
        public static void Serialize(string file, object instance, PreFieldSerializingDelegate callback)
        {
            Serialize(file, instance, string.Empty, callback);
        }
        public static void Serialize(string file, object instance, string fieldPrefix, PreFieldSerializingDelegate callback)
        {
            using (var writer = new JsonWriter(file))
            {
                JsonSerializer.Serialize(writer, instance, fieldPrefix, callback);
            }
        }
        public static string Serialize(object instance, PreFieldSerializingDelegate callback)
        {
            return Serialize(instance, string.Empty, callback);
        }
        public static string Serialize(object instance, string fieldPrefix, PreFieldSerializingDelegate callback)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new JsonWriter(sw))
            {
                JsonSerializer.Serialize(writer, instance, fieldPrefix, callback);
                return sw.ToString();
            }
        }
        
        

        public static T Deserialize<T>(Stream input)
        {
            return Deserialize<T>(input, string.Empty);
        }
        public static T Deserialize<T>(Stream input, string fieldPrefix)
        {
            using (var reader = new JsonReader(input))
            {
                return JsonDeserializer.Deserialize<T>(reader, fieldPrefix);
            }
        }
        public static T DeserializeFromFile<T>(string file)
        {
            return DeserializeFromFile<T>(file, string.Empty);
        }
        public static T DeserializeFromFile<T>(string file, string fieldPrefix)
        {
            using (var reader = new JsonReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                return JsonDeserializer.Deserialize<T>(reader, fieldPrefix);
            }  
        }
        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, string.Empty);
        }
        public static T Deserialize<T>(string json, string fieldPrefix)
        {
            using (var sr = new StringReader(json))
            using (var reader = new JsonReader(sr))
            {
                return JsonDeserializer.Deserialize<T>(reader, fieldPrefix);                
            }
        }
    }
}
