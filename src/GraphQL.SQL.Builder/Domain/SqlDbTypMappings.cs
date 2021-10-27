using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GraphQL.SQL.Builder.Domain
{
    public static class SqlDbTypeExtensions
    {
        private static Dictionary<string, SqlDbType> _typeMappings;
        public static SqlDbType ToSqlDbType(this string sqlTypeString)
        {
            if (_typeMappings == null)
            {
                _typeMappings = new Dictionary<string, SqlDbType>(StringComparer.OrdinalIgnoreCase);
                foreach (SqlDbType t in Enum.GetValues(typeof(SqlDbType)))
                {
                    _typeMappings.Add(t.ToString().ToLowerInvariant(), t);
                }
            }

            if (_typeMappings.ContainsKey(sqlTypeString))
            {
                return _typeMappings[sqlTypeString];
            }

            return SqlDbType.NVarChar;
        }
    }
}
