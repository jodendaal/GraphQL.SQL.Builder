namespace GraphQL.SQL.Builder
{
    public class UpdateField : Field
    {
        public UpdateField(string name, string value)
            : base(name)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}
