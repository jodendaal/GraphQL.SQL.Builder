namespace GraphQL.SQL.Builder
{
    public class Field
    {
        public Field(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class InsertField : Field
    {
        public InsertField(string name,string value):base(name)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; }
    }
}

