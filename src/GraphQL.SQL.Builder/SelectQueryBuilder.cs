using GraphQL.SQL.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{

    public class SelectQueryBuilder : BaseBuilder
    {
        private readonly string _tableName;
        private readonly string _tableAlias;
        List<SelectField> _fields = new List<SelectField>();
        List<SelectJoin> _joins = new List<SelectJoin>();
        List<SelectCondition> _conditions = new List<SelectCondition>();
        Dictionary<int,SelectConditionSet> _conditionSets = new Dictionary<int, SelectConditionSet>();
        private string _pageNumber;
        private string _pageSize;
        private bool _pagingEnabled = false;
        private string _pagingOrderBy = "";
        private string _orderBy;
        private string _groupBy;

        public string TableAlias => _tableAlias;

        public SelectQueryBuilder(string tableName, string tableAlias,string parameterPrefix = ""):base(parameterPrefix)
        {
            this._tableName = tableName;
            this._tableAlias = tableAlias;
        }

        public SelectQueryBuilder(string tableName,string tableAlias = "")
        {
            this._tableName = tableName;
            this._tableAlias = tableAlias;
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

        public SelectQueryBuilder OrderBy(string fields,string direction = "ASC")
        {
            _orderBy = $"ORDER BY {fields} {direction}";
            return this;
        }

        public SelectQueryBuilder Join(string tableName, JoinType joinType, string joinFields)
        {
            return Join(new SelectJoin(tableName, joinType, joinFields));
        }

        public SelectQueryBuilder Join(SelectJoin field)
        {
            _joins.Add(field);
            return this;
        }

        public SelectQueryBuilder ConditionSet(int id,SetOperator setOperator,Action<SelectConditionSet> func)
        {
            if (!_conditionSets.ContainsKey(id))
            {
                _conditionSets.Add(id, new SelectConditionSet(setOperator));
                
            }
            func(_conditionSets[id]);
            return this;
        }

        public virtual SelectQueryBuilder Page(string pageNumber, string pageSize,string orderBy = "")
        {
            _pagingEnabled = true;
            _pageSize = pageSize;
            _pageNumber = pageNumber;
            _pagingOrderBy = orderBy;
            return this;
        }

        public SelectQueryBuilder Condition(string fieldName, ColumnOperator @operator, string value)
        {
            return Condition(new SelectCondition(fieldName, @operator, value));
        }

        public virtual SelectQueryBuilder Condition(SelectCondition field)
        {
            _conditions.Add(field);
            return this;
        }

        public SelectQueryBuilder AddField(SelectField field)
        {
            _fields.Add(field);
            return this;
        }
        

        public string GetSetsSql(Dictionary<int,SelectConditionSet> filterSets)
        {

            var setClauses = new List<KeyValuePair<SetOperator, string>>();
            foreach (var set in filterSets)
            {
                setClauses.Add(new KeyValuePair<SetOperator, string>(set.Value.SetOperator, set.Value.GetSetSql(set.Value, set.Key)));
            }

            var result = "";
            if (setClauses.Count() > 1)
            {
                //Group By SetOperator
                //Ands First then ORS
                var results = from p in setClauses
                              group p by p.Key into g
                              select new
                              {
                                  Key = g.Key,
                                  Items = g.ToList()
                              };

                var ands = results.Where(i => i.Key == SetOperator.And).Select(i => i.Items).FirstOrDefault();
                if (ands != null)
                {
                    var andSql = ands.Select(i => $"({i.Value})");
                    result = string.Join($" {SetOperator.And}{Environment.NewLine}", andSql);
                }


                var ors = results.Where(i => i.Key == SetOperator.Or).Select(i => i.Items).FirstOrDefault();
                if (ors != null)
                {
                    var orSql = ors.Select(i => $"({i.Value})");
                    if (ors.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(result))
                        {
                            result = $"{string.Join($" {SetOperator.Or}{Environment.NewLine}", orSql)}";

                        }
                        else
                        {
                            result = $"({result}) OR{Environment.NewLine}{string.Join($" {SetOperator.Or}", orSql)}";
                        }
                    }
                }
            }
            else
            {
                result = string.Join(" ", setClauses.Select(i => $"{i.Value}"));
            }

            return result;
        }


        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            //Select
            result.Append($"SELECT{Environment.NewLine}");

            //Fields
            var fields = string.Join($",{Environment.NewLine}", _fields.Select(i => $"{i}"));
            result.Append($"{fields}{Environment.NewLine}");
           
            //From
            result.Append($"FROM ");
            if (!string.IsNullOrWhiteSpace(TableAlias))
            {
                result.Append($"{_tableName} {TableAlias}{Environment.NewLine}");
            }
            else
            {
                result.Append($"{_tableName}{Environment.NewLine}");
            }

            //Joins
            if (_joins.Count > 0)
            {
                var joins = _joins.Select(i => $"{i}");
                result.Append(string.Join($"{Environment.NewLine}", joins));
                result.AppendLine();
            }

            //Where
            if (_conditions.Count > 0 || _conditionSets.Count > 0)
            {
                result.Append($"WHERE ");
                if (_conditions.Count > 0)
                {
                    if (_conditionSets.Count > 0 && _conditions.Count > 0)
                    {
                        result.Append($"(");
                    }

                    result.Append(string.Join($" {SetOperator.And} ", _conditions.Select(i => i.ToString())));

                    if (_conditionSets.Count > 0 && _conditions.Count > 0)
                    {
                        result.Append($")");
                    }
                }

                if (_conditionSets.Count > 0)
                {
                    var conditionSetSql = GetSetsSql(_conditionSets);
                    if (_conditions.Count > 0) {

                        result.Append($" AND{Environment.NewLine}({conditionSetSql})");
                    }
                    else
                    {
                        result.Append($"{conditionSetSql}");
                    }
                }
            }

            //Group By
            if (!string.IsNullOrWhiteSpace(_groupBy))
            {
                result.AppendLine($@"{_groupBy}");
            }

            //Order By
            if (!string.IsNullOrWhiteSpace(_orderBy))
            {
                result.AppendLine($@"{_orderBy}");
            }

            //Paging
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
