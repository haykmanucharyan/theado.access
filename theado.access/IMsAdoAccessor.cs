using System.Collections.Generic;
using System.Data;

namespace theadoaccess
{
    public interface IMsAdoAccessor : IAdoAccessor
    {
        List<T> ExecSP2Table<T>(string schema, string spName, int timeout, params IDataParameter[] parameters) where T : class;

        List<T> ExecSP2Table<T>(string schema, string spName, int timeout, out IDbTransaction transaction, params IDataParameter[] parameters) where T : class;
    }
}
