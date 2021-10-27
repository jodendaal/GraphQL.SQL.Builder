namespace GraphQL.SQL.Builder
{
    public class SelectField
    {
        public SelectField(string name, string @as)
        {
            Name = name;
            As = @as;
        }

        public string Name { get; set; }
        public string As { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(As))
            {
                return $"{Name} AS {As}";
            }

            return $"{Name}";
        }
    }
}

