using System.Collections.Generic;
using System.Data;
using themapper;

namespace theadoaccess
{
    public interface IAdoAccessor
    {
        IDBMapper Mapper { get; }

        string ConnectionString { get; }

        DataTable Map2Table<T>(IEnumerable<T> entities) where T : class;

        IDbConnection CreateConnection(bool open);

        IDataParameter CreateParameter(string name, DbType dbType, object value);

        IDataParameter CreateParameter(string name, object value);

        void ExecCmd(string commandText, int timeout, params IDataParameter[] parameters);

        void ExecCmd(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters);

        List<T> ExecCmd2Table<T>(string commandText, int timeout, params IDataParameter[] parameters) where T: class;

        List<T> ExecCmd2Table<T>(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters) where T : class;

        T ExecCmd2Scalar<T>(string commandText, int timeout, params IDataParameter[] parameters);

        T ExecCmd2Scalar<T>(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters);

        void ExecSP(string schema, string spName, int timeout, params IDataParameter[] parameters);

        void ExecSP(string schema, string spName, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters);

        List<T> Fn<T>(string schema, string fnName, int timeout, params IDataParameter[] parameters) where T : class;

        T ScalarFn<T>(string schema, string fnName, int timeout, params IDataParameter[] parameters);
    }
}