using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{
    public class DeleteQueryBuilder : BaseConditionBuilder<DeleteQueryBuilder>
    {
        private string _tableAlias;
        private readonly string _tableName;
       
       
        public DeleteQueryBuilder(string tableName,string tableAlias = "")
        {
            _tableName = tableName;
            _tableAlias = tableAlias;
        }
              
        public override string ToString()
        {
            var sql = new StringBuilder();
            var isSelect = this.Joins.Count > 0;

            if(this.Joins.Count == 0)
            {
                sql.AppendLine($"DELETE FROM {_tableName}");
            }
            else
            {
                sql.AppendLine($"DELETE {_tableAlias} FROM {_tableName} {_tableAlias}");
            }

            //Joins and Where
            var where = base.ToString();
            sql.Append(where);

            return sql.ToString();
        }
    }
}
