using GraphQL.SQL.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.SQL
{
    public class SelectConditionSet
    {
        public SelectConditionSet(SetOperator setOperator)
        {
            SetOperator = setOperator;
        }

        public List<SelectCondition> And { get; set; } = new List<SelectCondition>();

        public List<SelectCondition> Or { get; set; } = new List<SelectCondition>();

        public SetOperator SetOperator { get; set; } = SetOperator.Or;

        public SelectConditionSet AndSet { get; set; }

        public SelectConditionSet OrSet { get; set; }

        public SelectConditionSet AndCondition(string fieldName, ColumnOperator @operator, string value)
        {
            And.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet OrCondition(string fieldName, ColumnOperator @operator, string value)
        {
            Or.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet OrSetOrCondition(string fieldName, ColumnOperator @operator, string value)
        {
            if (OrSet == null)
            {
                OrSet = new SelectConditionSet(SetOperator.Or);
            }

            OrSet.Or.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet OrSetAndCondition(string fieldName, ColumnOperator @operator, string value)
        {
            if (OrSet == null)
            {
                OrSet = new SelectConditionSet(SetOperator.Or);
            }

            OrSet.And.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet AndSetAndCondition(string fieldName, ColumnOperator @operator, string value)
        {
            if (AndSet == null)
            {
                AndSet = new SelectConditionSet(SetOperator.And);
            }

            AndSet.And.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet GetAndSet(string fieldName, string @operator, string value)
        {
            if (AndSet == null)
            {
                AndSet = new SelectConditionSet(SetOperator.Or);
            }

            return AndSet;
        }

        public SelectConditionSet GetOrSet(string fieldName, string @operator, string value)
        {
            if (OrSet == null)
            {
                OrSet = new SelectConditionSet(SetOperator.Or);
            }

            return OrSet;
        }

        public virtual string GetSetSql(SelectConditionSet set, int level)
        {
            var andFilter = string.Empty;
            for (int i = 0; i < set.And.Count; i++)
            {
                var item = set.And[i];
                andFilter += i == 0 ? $"{item}" : $" AND {item}";
            }

            var filterResult = andFilter;

            level++;
            var orFilter = string.Empty;
            for (int i = 0; i < set.Or.Count; i++)
            {
                var item = set.Or[i];
                orFilter += i == 0 ? $"{item}" : $" OR {item}";
            }

            if (!string.IsNullOrWhiteSpace(orFilter))
            {
                filterResult = string.IsNullOrWhiteSpace(filterResult) ? orFilter : $"(({andFilter}) {set.SetOperator} ({orFilter}))";
            }

            if (set.AndSet != null)
            {
                var andSetString = GetSetSql(set.AndSet, level + 1);
                filterResult = $"({filterResult}) AND{Environment.NewLine}({andSetString})";
            }

            if (set.OrSet != null)
            {
                var orSetString = GetSetSql(set.OrSet, level + 2);
                filterResult = $"({filterResult}) OR{Environment.NewLine}({orSetString})";
            }

            return $"{filterResult}";
        }
    }
}
