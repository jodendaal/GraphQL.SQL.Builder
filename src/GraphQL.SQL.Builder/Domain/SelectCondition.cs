using GraphQL.SQL.Builder;

namespace GraphQL.SQL.Builder
{
    public class SelectCondition
    {
        public SelectCondition(string fieldName, string @operator, string value)
        {
            FieldName = fieldName;
            Operator = @operator;
            Value = value;
        }

        public string FieldName { get; set; }

        public string Operator { get; set; } = ColumnOperator.Equals;

        public string Value { get; set; }

        public override string ToString()
        {
            switch (Operator)
            {
                case ColumnOperator.NOT_EXISTS:
                case ColumnOperator.EXISTS:
                    return $"{Operator} {Value}";
                default:
                    return $"{FieldName} {Operator} {Value}";
            }
        }
    }
}
