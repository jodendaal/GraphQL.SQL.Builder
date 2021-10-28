using GraphQL.SQL.Builder;

namespace GraphQL.SQL.Builder
{
    public class SelectCondition
    {
        public SelectCondition(string fieldName, ColumnOperator @operator, string value)
        {
            FieldName = fieldName;
            Operator = @operator;
            Value = value;
        }

        public string FieldName { get; set; }

        public ColumnOperator Operator { get; set; } = ColumnOperator.Equals;

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{FieldName} {Operator} {Value}";
        }
    }
}
