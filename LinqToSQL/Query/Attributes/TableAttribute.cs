namespace LinqToSQL.Query.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class TableAttribute : Attribute
{
    public string Name { get; }
    public TableAttribute(string name) => Name = name;
}
