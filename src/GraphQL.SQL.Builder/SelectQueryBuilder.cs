using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{
    public class SelectQueryBuilder : BaseConditionBuilder<SelectQueryBuilder>
    {
        private string _tableName;
        private string _tableAlias;
        private readonly List<SelectField> _fields = new List<SelectField>();

        private string _pageNumber;
        private string _pageSize;
        private bool _pagingEnabled = false;
        private string _pagingOrderBy = string.Empty;
        private string _orderBy;
        private string _groupBy;

        public string TableAlias => _tableAlias;

        public SelectQueryBuilder(string tableName, string tableAlias = "")
        {
            this._tableName = tableName;
            this._tableAlias = tableAlias;
        }

        public SelectQueryBuilder Table(string tableName, string tableAlias)
        {
            _tableAlias = tableAlias;
            _tableName = tableName;
            return this;
        }

        public SelectQueryBuilder Max(string name, string @as = "")
        {
            return Field(new SelectField($"MAX({name})", @as));
        }

        public SelectQueryBuilder Min(string name, string @as = "")
        {
            return Field(new SelectField($"MIN({name})", @as));
        }

        public SelectQueryBuilder Sum(string name, string @as = "")
        {
            return Field(new SelectField($"SUM({name})", @as));
        }

        public SelectQueryBuilder Avg(string name, string @as = "")
        {
            return Field(new SelectField($"AVG({name})", @as));
        }

        public SelectQueryBuilder Count(string name, string @as = "")
        {
            return Field(new SelectField($"COUNT({name})", @as));
        }

        public SelectQueryBuilder Field(string name, string @as = "")
        {
            return Field(new SelectField(name, @as));
        }

        public SelectQueryBuilder Field(SelectField field)
        {
            _fields.Add(field);
            return this;
        }

        public SelectQueryBuilder GroupBy(string fields)
        {
            _groupBy = $"GROUP BY {fields}";
            return this;
        }

        public SelectQueryBuilder OrderByAsc(string fields)
        {
            return OrderBy(fields, "ASC");
        }

        public SelectQueryBuilder OrderByDesc(string fields)
        {
            return OrderBy(fields, "DESC");
        }

        public SelectQueryBuilder OrderBy(string fields, string direction = "ASC")
        {
            _orderBy = $"ORDER BY {fields} {direction}";
            return this;
        }

        public virtual SelectQueryBuilder Page(string pageNumber, string pageSize, string orderBy = "")
        {
            _pagingEnabled = true;
            _pageSize = pageSize;
            _pageNumber = pageNumber;
            _pagingOrderBy = orderBy;
            return this;
        }

        public SelectQueryBuilder AddField(SelectField field)
        {
            _fields.Add(field);
            return this;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            // Select
            result.Append($"SELECT{Environment.NewLine}");

            // Fields
            var fields = string.Join($",{Environment.NewLine}", _fields.Select(i => $"{i}"));
            result.Append($"{fields}{Environment.NewLine}");

            // From
            result.Append($"FROM ");
            if (!string.IsNullOrWhiteSpace(TableAlias))
            {
                result.Append($"{_tableName} {TableAlias}{Environment.NewLine}");
            }
            else
            {
                result.Append($"{_tableName}{Environment.NewLine}");
            }

            // Joins and Where
            var where = base.ToString();
            result.Append(where);

            // Group By
            if (!string.IsNullOrWhiteSpace(_groupBy))
            {
                result.AppendLine($@"{_groupBy}");
            }

            // Order By
            if (!string.IsNullOrWhiteSpace(_orderBy))
            {
                result.AppendLine($@"{_orderBy}");
            }

            // Paging
            if (_pagingEnabled)
            {
                if (string.IsNullOrWhiteSpace(_orderBy))
                {
                    result.AppendLine($@"{Environment.NewLine}ORDER BY {_pagingOrderBy}");
                }

                result.AppendLine($@"OFFSET {_pageSize} * ({_pageNumber} - 1)");
                result.Append($@"ROWS FETCH NEXT {_pageSize} ROWS ONLY");
            }

            return result.ToString();
        }
    }
}
