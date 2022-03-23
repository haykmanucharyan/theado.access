namespace theadoaccess
{
    public class DBServerFactory : IDBServerFactory
    {
        public IAdoAccessor CreatePostgreSqlAccessor(string connectionString)
        {
            return new PostgreSqlAccessor(connectionString);
        }

        public IMsAdoAccessor CreateMsSqlAccessor(string connectionString)
        { 
            return new MsSqlAdoAccessor(connectionString);
        }
    }
}
