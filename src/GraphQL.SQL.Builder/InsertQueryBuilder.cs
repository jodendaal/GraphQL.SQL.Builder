using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{
    public class InsertQueryBuilder : BaseBuilder<InsertQueryBuilder>
    {
        private readonly string _tableName;
        private readonly List<InsertField> _fields = new List<InsertField>();
        private SelectQueryBuilder _from;

        public InsertQueryBuilder(string tableName)
        {
            _tableName = tableName;
        }

        public InsertQueryBuilder From(string tableName, string tableAlias, Action<SelectQueryBuilder> func)
        {
            _from = new SelectQueryBuilder(tableName, tableAlias);
            func(_from);
            return this;
        }

        public InsertQueryBuilder Field(string name)
        {
            return Field(new InsertField(name, string.Empty));
        }

        public InsertQueryBuilder Field(string name, string value)
        {
            return Field(new InsertField(name, value));
        }

        public InsertQueryBuilder Field(InsertField field)
        {
            _fields.Add(field);
            return this;
        }

        public override string ToString()
        {
            var sql = new StringBuilder();
            sql.AppendLine($"INSERT INTO {_tableName}");

            // Fields
            if (_fields.Count > 0)
            {
                sql.AppendLine("(");
                sql.AppendLine(string.Join($",{Environment.NewLine}", _fields.Select(i => i.Name)));
                sql.AppendLine(")");

                if (_from == null)
                {
                    sql.AppendLine("VALUES");
                    sql.AppendLine("(");
                    sql.AppendLine(string.Join($",{Environment.NewLine}", _fields.Select(i => i.Value)));
                    sql.AppendLine(")");
                }
            }

            // From
            if (_from != null)
            {
                sql.AppendLine(_from.ToString());
            }

            return sql.ToString();
        }
    }
}
