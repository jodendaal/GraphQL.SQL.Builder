using System.Collections.Generic;
using System.Data.SqlClient;
namespace GraphQL.SQL.Builder
{
    public class SelectQueryCommand : SelectQueryBuilder
    {
        Dictionary<string, System.Data.SqlClient.SqlParameter> _parameters = new Dictionary<string, System.Data.SqlClient.SqlParameter>();
        public Dictionary<string, System.Data.SqlClient.SqlParameter> Parameters { get { return _parameters; } }

        public bool AddParams { get; private set; }

        int paramCount = 0;

        public SelectQueryCommand(string tableName, string tableAlias = "") : base(tableName, tableAlias)
        {
        }

        /// <summary>
        /// Adds a command parameters and returns the new paramter name
        /// Optional name param will only be used if it does not already exist
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AddParam(object value, string name = "",string type = null)
        {
            var paramName = name == string.Empty ? $"@p_{paramCount}" : $"@{name}";
            SqlParameter paramter;
            if (_parameters.ContainsKey(paramName))
            {
                paramName = $"{paramName}_{paramCount}";
            }

            paramter = new SqlParameter(paramName, value);
            //paramter.SqlDbType = QQQQ
            _parameters.Add(paramName, paramter);
            paramCount = paramCount + 1;

            return paramName;
        }

        public SqlCommand ToCommand()
        {
            var sqlCommand = new SqlCommand(this.ToString());
            foreach(var param in this.Parameters)
            {
                sqlCommand.Parameters.Add(param.Value);
            }

            return sqlCommand;
        }

    }
}
