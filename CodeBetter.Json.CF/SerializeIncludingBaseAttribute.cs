namespace CodeBetter.Json
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SerializeIncludingBaseAttribute : Attribute { }
}
