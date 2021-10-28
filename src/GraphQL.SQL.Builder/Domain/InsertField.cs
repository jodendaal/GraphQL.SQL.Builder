namespace GraphQL.SQL.Builder
{
    public class InsertField : Field
    {
        public InsertField(string name, string value)
            : base(name)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; }
    }
}
