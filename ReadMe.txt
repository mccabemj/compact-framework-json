JsonCF is lighweight and easy to use. It can be used from both the Compact Framework (v2.0+) or the .NET framework (v2.0+). The source
code leverages linked-files to build assemblies which target each framework.

The quickest way to get started is by using the Serialize and Deserialize static methods which accepts an object and a string respectively:

//serialize
string json = Converter.Serialize(new User("name", "password", AccountStatus.Enabled));
//or to a file
Converter.Serialize("out.txt", new int[]{1,2,3});

//deserialize
User user = Converter.Deserialize<User>(json);
//or from a file
int[] values = Converter.DeserializeFormFile<int[]>("out.txt");

Overloads are available to serialize/desirialize from a stream or a file (to deserialize from a file, use the DeserializeFromFile static method)

If you don't want a field serialized, mark it with the System.NonSerializedAttribute.

If you want to also include the fields from the base-class, mark the class witht he CodeBetter.Json.SerializeIncludingBase attribute. This must
be placed on each class in the hierachy. Once a class is found that doesn't have the attribute, we stop navigating down the field.

I do want to work out a good naming strategy. For now, the static Serialize and Deserialize methods accept a last parameter called "FieldPrefix". 
When serializing, this value will be stripped from the names (_userName --> userName). While deserializing, the prefix is appending (assuming
it isn't already there).



