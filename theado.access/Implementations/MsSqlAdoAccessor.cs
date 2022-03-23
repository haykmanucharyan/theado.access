using System.Data;
using System.Data.SqlClient;
using System.Text;
using themapper;

namespace theadoaccess
{
    internal class MsSqlAdoAccessor : BaseAdoAccessor, IMsAdoAccessor
    {
        #region Ctor

        public MsSqlAdoAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        #endregion

        #region Methods

        public override IDbConnection CreateConnection(bool open)
        {
            IDbConnection connection = new SqlConnection(ConnectionString);

            if (open)
                connection.Open();

            return connection;
        }

        public override IDataParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }
                              
        public override IDataParameter CreateParameter(string name, DbType dbType, object value)
        {
            IDataParameter parameter = new SqlParameter();
            parameter.ParameterName = name;
            parameter.DbType = dbType;
            parameter.Value = value;

            return parameter;
        }

        #endregion
    }
}
