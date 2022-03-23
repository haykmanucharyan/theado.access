using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using themapper;

namespace theadoaccess
{
    internal abstract class BaseAdoAccessor : IAdoAccessor
    {
        #region Fields

        protected string _connectionString = string.Empty;

        #endregion

        #region Properties

        public IDBMapper Mapper => DBMapper.Instance;

        public string ConnectionString => _connectionString;

        #endregion

        #region Abstract methods

        public abstract IDbConnection CreateConnection(bool open);

        public abstract IDataParameter CreateParameter(string name, object value);

        public abstract IDataParameter CreateParameter(string name, DbType dbType, object value);

        #endregion

        #region Private methods

        private void ExecCmd(string commandText, int timeout, bool beginTran, out IDbTransaction transaction, params IDataParameter[] parameters)
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                transaction = GetTranIfNeeded(beginTran, connection);

                using (IDbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = timeout;
                    cmd.CommandType = CommandType.Text;

                    if (beginTran)
                        cmd.Transaction = transaction;

                    FillParams(cmd, parameters);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<T> ExecCmd2Table<T>(string commandText, int timeout, bool beginTran, out IDbTransaction transaction, IDataParameter[] parameters) where T : class
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                transaction = GetTranIfNeeded(beginTran, connection);

                using (IDbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = timeout;
                    cmd.CommandType = CommandType.Text;

                    if (beginTran)
                        cmd.Transaction = transaction;

                    FillParams(cmd, parameters);

                    using (IDataReader rdr = cmd.ExecuteReader())
                        return Mapper.Map2List<T>(rdr);
                }
            }
        }

        private void ExecSP(string schema, string spName, int timeout, bool beginTran, out IDbTransaction transaction, IDataParameter[] parameters)
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                transaction = GetTranIfNeeded(beginTran, connection);

                using (IDbCommand cmd = connection.CreateCommand())
                {
                    InitExecSPCommand(cmd, timeout, schema, spName, parameters);

                    if (beginTran)
                        cmd.Transaction = transaction;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<T> ExecSP2Table<T>(string schema, string spName, int timeout, bool beginTran, out IDbTransaction transaction, params IDataParameter[] parameters) where T : class
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                transaction = GetTranIfNeeded(beginTran, connection);

                using (IDbCommand cmd = connection.CreateCommand())
                {
                    InitExecSPCommand(cmd, timeout, schema, spName, parameters);

                    if (beginTran)
                        cmd.Transaction = transaction;

                    using (IDataReader rdr = cmd.ExecuteReader())
                        return Mapper.Map2List<T>(rdr);
                }
            }
        }

        #endregion

        #region Protected methods

        protected T GetValueFromScalarExecution<T>(IDbCommand cmd)
        {
            object? val = cmd.ExecuteScalar();

#pragma warning disable

            if (val is null || val == DBNull.Value)
                return default;

            return (T)cmd.ExecuteScalar();

#pragma warning restore
        }

        protected IDbTransaction GetTranIfNeeded(bool beginTran, IDbConnection connection)
        {
            if (beginTran)
                return connection.BeginTransaction();

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        protected void FillParams(IDbCommand cmd, IDataParameter[] parameters)
        {
            if (parameters?.Length > 0)
                foreach (IDataParameter prm in parameters)
                    cmd.Parameters.Add(prm);
        }

        protected virtual void InitExecSPCommand(IDbCommand cmd, int timeout, string schema, string spName, params IDataParameter[] parameters)
        {
            cmd.CommandText = $"{schema}.{spName}";
            cmd.CommandTimeout = timeout;
            cmd.CommandType = CommandType.StoredProcedure;

            FillParams(cmd, parameters);
        }

        protected virtual string ConstructFnString(string schema, string fnName, params IDataParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT * FROM {0}.{1}(", schema, fnName);

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

        protected virtual string ConstructScalarFnString(string schema, string fnName, params IDataParameter[] parameters)
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

        #region IAdoAccessor implementation

        public DataTable Map2Table<T>(IEnumerable<T> entities) where T : class
        {
            return Mapper.ToTable<T>(entities);
        }

        public virtual void ExecCmd(string commandText, int timeout, params IDataParameter[] parameters)
        {
            ExecCmd(commandText, timeout, false, out _, parameters);
        }

        public virtual void ExecCmd(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters)
        {
            ExecCmd(commandText, timeout, true, out transaction, parameters);
        }

        private T ExecCmd2Scalar<T>(string commandText, int timeout, bool beginTran, out IDbTransaction transaction, params IDataParameter[] parameters)
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                transaction = GetTranIfNeeded(beginTran, connection);

                using (IDbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = timeout;
                    cmd.CommandType = CommandType.Text;

                    if (beginTran)
                        cmd.Transaction = transaction;

                    FillParams(cmd, parameters);

                    return GetValueFromScalarExecution<T>(cmd);
                }
            }
        }

        public virtual T ExecCmd2Scalar<T>(string commandText, int timeout, params IDataParameter[] parameters)
        {
            return ExecCmd2Scalar<T>(commandText, timeout, false, out _, parameters);
        }

        public virtual T ExecCmd2Scalar<T>(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters)
        {
            return ExecCmd2Scalar<T>(commandText, timeout, true, out transaction, parameters);
        }        

        public virtual List<T> ExecCmd2Table<T>(string commandText, int timeout, params IDataParameter[] parameters) where T : class
        {
            return ExecCmd2Table<T>(commandText, timeout, false, out _, parameters);
        }

        public virtual List<T> ExecCmd2Table<T>(string commandText, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters) where T : class
        {
            return ExecCmd2Table<T>(commandText, timeout, true, out transaction, parameters);
        }

        public virtual void ExecSP(string schema, string spName, int timeout, params IDataParameter[] parameters)
        {
            ExecSP(schema, spName, timeout, false, out _, parameters);
        }

        public virtual void ExecSP(string schema, string spName, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters)
        {
            ExecSP(schema, spName, timeout, true, out transaction, parameters);
        }        

        public List<T> ExecSP2Table<T>(string schema, string spName, int timeout, params IDataParameter[] parameters) where T : class
        {
            return ExecSP2Table<T>(schema, spName, timeout, false, out _, parameters);
        }

        public List<T> ExecSP2Table<T>(string schema, string spName, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters) where T : class
        {
            return ExecSP2Table<T>(schema, spName, timeout, true, out transaction, parameters);
        }

        public virtual List<T> Fn<T>(string schema, string fnName, int timeout, params IDataParameter[] parameters) where T : class
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                using (IDbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = ConstructFnString(schema, fnName, parameters);
                    cmd.CommandTimeout = timeout;
                    cmd.CommandType = CommandType.Text;

                    FillParams(cmd, parameters);

                    using (IDataReader rdr = cmd.ExecuteReader())
                        return Mapper.Map2List<T>(rdr);
                }
            }
        }

        public virtual T ScalarFn<T>(string schema, string fnName, int timeout, params IDataParameter[] parameters)
        {
            using (IDbConnection connection = CreateConnection(true))
            {
                using (IDbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = ConstructScalarFnString(schema, fnName, parameters);
                    cmd.CommandTimeout = timeout;
                    cmd.CommandType = CommandType.Text;

                    FillParams(cmd, parameters);

                    return GetValueFromScalarExecution<T>(cmd);
                }
            }
        }

        #endregion
    }
}
