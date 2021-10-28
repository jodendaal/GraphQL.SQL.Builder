using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQL.SQL.Builder
{
    public class BaseConditionBuilder<T> : BaseBuilder<T>
        where T : BaseBuilder<T>
    {
        private readonly List<SelectCondition> _conditions = new List<SelectCondition>();
        private readonly Dictionary<int, SelectConditionSet> _conditionSets = new Dictionary<int, SelectConditionSet>();
        private readonly List<SelectJoin> _joins = new List<SelectJoin>();

        internal List<SelectCondition> Conditions { get => _conditions; }

        internal Dictionary<int, SelectConditionSet> ConditionSets { get => _conditionSets; }

        internal List<SelectJoin> Joins { get => _joins; }

        public BaseConditionBuilder()
        {
        }

        public T ConditionSet(int id, string setOperator, Action<SelectConditionSet> func)
        {
            if (!_conditionSets.ContainsKey(id))
            {
                _conditionSets.Add(id, new SelectConditionSet(setOperator));
            }

            func(_conditionSets[id]);

            return this as T;
        }

        public T Condition(SelectCondition field)
        {
            Conditions.Add(field);
            return this as T;
        }

        public T Exists(Action<SelectQueryBuilder> builder)
        {
            var queryBuilder = new SelectQueryBuilder(string.Empty, string.Empty);
            builder(queryBuilder);
            Condition(new SelectCondition(string.Empty, ColumnOperator.EXISTS, $"({queryBuilder})"));
            return this as T;
        }

        public T NotExists(Action<SelectQueryBuilder> builder)
        {
            var queryBuilder = new SelectQueryBuilder(string.Empty, string.Empty);
            builder(queryBuilder);
            Condition(new SelectCondition(string.Empty, ColumnOperator.NOT_EXISTS, $"({queryBuilder})"));
            return this as T;
        }

        public T In(string fieldName, Action<SelectQueryBuilder> builder)
        {
            var queryBuilder = new SelectQueryBuilder(string.Empty, string.Empty);
            builder(queryBuilder);
            return In(fieldName, queryBuilder.ToString());
        }

        public T In(string fieldName, string value)
        {
            Condition(new SelectCondition(fieldName, ColumnOperator.IN, $"({value})"));
            return this as T;
        }

        public T NotIn(string fieldName, Action<SelectQueryBuilder> builder)
        {
            var queryBuilder = new SelectQueryBuilder(string.Empty, string.Empty);
            builder(queryBuilder);
            return NotIn(fieldName, queryBuilder.ToString());
        }

        public T NotIn(string fieldName, string value)
        {
            Condition(new SelectCondition(fieldName, ColumnOperator.NOT_IN, $"({value})"));
            return this as T;
        }

        public T Condition(string fieldName, string @operator, Action<SelectQueryBuilder> builder)
        {
            var queryBuilder = new SelectQueryBuilder(string.Empty, string.Empty);
            builder(queryBuilder);
            Condition(new SelectCondition(fieldName, @operator, $"({queryBuilder})"));
            return this as T;
        }

        public T Condition(string rawSql)
        {
            Condition(new SelectCondition($"({rawSql})", string.Empty, string.Empty));
            return this as T;
        }

        public T Condition(string fieldName, string @operator, string value)
        {
            Condition(new SelectCondition(fieldName, @operator, value));
            return this as T;
        }

        public T InnerJoin(string tableName, string joinFields)
        {
            return Join(new SelectJoin(tableName, JoinType.Inner, joinFields));
        }

        public T LeftJoin(string tableName, string joinFields)
        {
            return Join(new SelectJoin(tableName, JoinType.Left, joinFields));
        }

        public T Join(string tableName, JoinType joinType, string joinFields)
        {
            return Join(new SelectJoin(tableName, joinType, joinFields));
        }

        public T Join(SelectJoin field)
        {
            Joins.Add(field);
            return this as T;
        }

        internal string GetSetsSql(Dictionary<int, SelectConditionSet> filterSets)
        {
            var setClauses = new List<KeyValuePair<string, string>>();
            foreach (var set in filterSets)
            {
                setClauses.Add(new KeyValuePair<string, string>(set.Value.SetOperator, set.Value.GetSetSql(set.Value, set.Key)));
            }

            var result = string.Empty;
            if (setClauses.Count > 1)
            {
                // Group By SetOperator
                // Ands First then ORS
                var results = from p in setClauses
                              group p by p.Key into g
                              select new
                              {
                                  g.Key,
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
            var result = new StringBuilder();

            // Joins
            if (Joins.Count > 0)
            {
                var joins = Joins.Select(i => $"{i}");
                result.Append(string.Join($"{Environment.NewLine}", joins));
                result.AppendLine();
            }

            // Conditions
            if (Conditions.Count > 0 || ConditionSets.Count > 0)
            {
                result.Append($"WHERE ");
                if (Conditions.Count > 0)
                {
                    if (Conditions.Count > 0 && ConditionSets.Count > 0)
                    {
                        result.Append($"(");
                    }

                    result.Append(string.Join($" {SetOperator.And} ", Conditions.Select(i => i.ToString())));

                    if (Conditions.Count > 0 && ConditionSets.Count > 0)
                    {
                        result.Append($")");
                    }
                }

                if (ConditionSets.Count > 0)
                {
                    var conditionSetSql = GetSetsSql(ConditionSets);
                    if (Conditions.Count > 0)
                    {
                        result.Append($" AND{Environment.NewLine}({conditionSetSql})");
                    }
                    else
                    {
                        result.Append($"{conditionSetSql}");
                    }
                }
            }

            return result.ToString();
        }
    }
}
