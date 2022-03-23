using Npgsql;
using System.Data;
using System.Text;

namespace theadoaccess
{
    internal class PostgreSqlAccessor : BaseAdoAccessor
    {
        #region Ctor

        public PostgreSqlAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        #endregion

        #region Methods        

        public override IDbConnection CreateConnection(bool open)
        {
            IDbConnection connection = new NpgsqlConnection(ConnectionString);

            if (open)
                connection.Open();

            return connection;
        }

        public override IDataParameter CreateParameter(string name, DbType dbType, object value)
        {
            IDataParameter parameter = new NpgsqlParameter(name, value);

            parameter.ParameterName = name;
            parameter.DbType = dbType;
            parameter.Value = value;

            return parameter;
        }

        public override IDataParameter CreateParameter(string name, object value)
        {
            return new NpgsqlParameter(name, value);
        }

        protected override void InitExecSPCommand(IDbCommand cmd, int timeout, string schema, string spName, params IDataParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters?.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i].ParameterName);

                    if (i != parameters.Length - 1)
                        sb.Append(",");

                    cmd.Parameters.Add(parameters[i]);
                }
            }

            cmd.CommandText = $"CALL {schema}.{spName}({sb})";            
            cmd.CommandTimeout = timeout;
            cmd.CommandType = CommandType.Text;
        }

        protected override string ConstructFnString(string schema, string fnName, params IDataParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT {0}.{1}(", schema, fnName);

            if (parameters?.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i].ParameterName);

                    if (i != parameters.Length - 1)
                        sb.Append(",");
                }
            }

            sb.Append(");");

            return sb.ToString();
        }

        #endregion
    }
}
