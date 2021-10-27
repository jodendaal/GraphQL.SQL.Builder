using System;

namespace GraphQL.SQL.Builder
{
    public sealed class JoinType
    {

        private readonly String name;
        private readonly int value;

        public static readonly JoinType Inner = new JoinType(1, "INNER");
        public static readonly JoinType Left = new JoinType(2, "LEFT");

        private JoinType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }

    }
}
