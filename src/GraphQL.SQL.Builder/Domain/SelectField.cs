namespace GraphQL.SQL.Builder
{
    public class SelectField : Field
    {
        public SelectField(string name, string @as)
            : base(name)
        {
            As = @as;
        }

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
