using System;

namespace GraphQL.SQL.Builder
{
    public sealed class ColumnOperator
    {
        private readonly string name;
        private readonly int value;

        public static readonly new ColumnOperator Equals = new ColumnOperator(1, "=");
        public static readonly ColumnOperator NotEquals = new ColumnOperator(3, "<>");
        public static readonly ColumnOperator GreaterThan = new ColumnOperator(2, ">");
        public static readonly ColumnOperator GreaterThanOrEqualTo = new ColumnOperator(4, ">=");
        public static readonly ColumnOperator LessThan = new ColumnOperator(5, "<");
        public static readonly ColumnOperator LessThanOrEqualTo = new ColumnOperator(6, "<=");

        private ColumnOperator(int value, string name)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return name;
        }

        public static ColumnOperator FromString(string operatorValue)
        {
            switch (operatorValue)
            {
                case "gt":
                    return ColumnOperator.GreaterThan;
                case "gte":
                    return ColumnOperator.GreaterThanOrEqualTo;
                case "lt":
                    return ColumnOperator.LessThan;
                case "lte":
                    return ColumnOperator.LessThanOrEqualTo;
                case "ne":
                    return ColumnOperator.NotEquals;
            }

            return ColumnOperator.Equals;
        }
    }
}
