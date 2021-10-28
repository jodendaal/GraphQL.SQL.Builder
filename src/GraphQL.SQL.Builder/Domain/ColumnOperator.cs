using System;

namespace GraphQL.SQL.Builder
{
    public sealed class ColumnOperator
    {
        public new const string Equals = "=";
        public const string NotEquals = "<>";
        public const string GreaterThan = ">";
        public const string GreaterThanOrEqualTo = ">=";
        public const string LessThan = "<";
        public const string LessThanOrEqualTo = "<=";
        public const string IN = "IN";
        public const string NOT_IN = "NOT IN";
        public const string EXISTS = "EXISTS";
    }
}
