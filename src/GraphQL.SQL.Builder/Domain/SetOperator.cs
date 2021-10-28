using System;

namespace GraphQL.SQL.Builder
{
    public sealed class SetOperator
    {
        private string name;
        private int value;

        public static readonly SetOperator Or = new SetOperator(1, "OR");
        public static readonly SetOperator And = new SetOperator(2, "AND");

        private SetOperator(int value, string name)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return name;
        }

        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
