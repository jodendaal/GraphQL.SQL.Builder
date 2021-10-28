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
}
