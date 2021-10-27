using GraphQL.SQL.Builder;
using System.Collections.Generic;

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
            And.Add(new SelectCondition(fieldName,@operator,value));
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
            if(AndSet == null)
            {
                AndSet = new SelectConditionSet(SetOperator.And);
            }

            AndSet.And.Add(new SelectCondition(fieldName, @operator, value));
            return this;
        }

        public SelectConditionSet GetAndSet(string fieldName, ColumnOperator @operator, string value)
        {
            if (AndSet == null)
            {
                AndSet = new SelectConditionSet(SetOperator.Or);
            }
           
            return AndSet;
        }

        public SelectConditionSet GetOrSet(string fieldName, ColumnOperator @operator, string value)
        {
            if (OrSet == null)
            {
                OrSet = new SelectConditionSet(SetOperator.Or);
            }

            return OrSet;
        }
    }
}

