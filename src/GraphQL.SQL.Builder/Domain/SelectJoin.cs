using GraphQL.SQL.Builder;

namespace GraphQL.SQL.Builder
{
    public class SelectJoin
    {
        public SelectJoin(string tableName, JoinType joinType,string joinFields)
        {
            TableName = tableName;
            JoinType = joinType;
            JoinFields = joinFields;
        }

        public string TableName { get; set; }
        public JoinType JoinType { get; set; }
        public string JoinFields { get; }

        public override string ToString()
        {
            return $"{JoinType} JOIN {TableName} ON {JoinFields}";
        }
    }
}

