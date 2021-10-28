using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{
    public class UpdateQueryBuilder : BaseConditionBuilder<UpdateQueryBuilder>
    {
        private string _tableAlias;
        private readonly string _tableName;
        List<UpdateField> _fields = new List<UpdateField>();
       
        public UpdateQueryBuilder(string tableName,string tableAlias = "")
        {
            _tableName = tableName;
            _tableAlias = tableAlias;
        }

        public UpdateQueryBuilder Field(string name, string value)
        {
            return Field(new UpdateField(name, value));
        }

        public UpdateQueryBuilder Field(UpdateField field)
        {
            _fields.Add(field);
            return this;
        }


        public override string ToString()
        {
            var sql = new StringBuilder();
            var isSelect = this.Joins.Count > 0;

            if(!isSelect)
            {
                sql.AppendLine($"UPDATE {_tableName}");
            }
            else
            {
                sql.AppendLine($"UPDATE {_tableAlias}");
            }

            //Fields
            if (_fields.Count > 0)
            {
                sql.AppendLine("SET");
                sql.AppendLine(string.Join($",{Environment.NewLine}", _fields.Select(i => i.ToString())));
            }

            if (isSelect)
            {
                sql.AppendLine($"FROM {_tableName} {_tableAlias}");
            }

            //Joins and Where
            var where = base.ToString();
            sql.Append(where);

            return sql.ToString();
        }

    }
}
