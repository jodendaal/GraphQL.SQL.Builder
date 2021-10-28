using GraphQL.SQL.Builder.Domain;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace GraphQL.SQL.Builder
{
    public class BaseBuilder<T> 
    {
        Dictionary<string, System.Data.SqlClient.SqlParameter> _parameters = new Dictionary<string, System.Data.SqlClient.SqlParameter>();
        public Dictionary<string, System.Data.SqlClient.SqlParameter> Parameters { get { return _parameters; } }
        int paramCount = 0;
        private readonly string parameterSuffix;
        public BaseBuilder()
        {

        }
        public BaseBuilder(string parameterSuffix)
        {
            this.parameterSuffix = parameterSuffix;
        }

        /// <summary>
        /// Adds a command parameters and returns the new paramter name
        /// Optional name param will only be used if it does not already exist
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AddParam(object value, SqlDbType type, string name = "")
        {
            var paramName = name == string.Empty ? $"@p_{paramCount}" : $"@{name}";
            if (!string.IsNullOrWhiteSpace(this.parameterSuffix))
            {
                paramName = $"{paramName}_{parameterSuffix}";
            }

            SqlParameter paramter;
            if (_parameters.ContainsKey(paramName))
            {
                paramName = $"{paramName}_{paramCount}";
            }

            paramter = new SqlParameter(paramName, value);
            paramter.SqlDbType = type;
            _parameters.Add(paramName, paramter);
            paramCount = paramCount + 1;

            return  paramName;
        }

        public string AddParam(object value, string name = "", string type = "nvarchar")
        {
            return AddParam(value,  type.ToSqlDbType(), name);
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
