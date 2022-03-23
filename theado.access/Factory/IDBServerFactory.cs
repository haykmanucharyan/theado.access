namespace theadoaccess
{
    public interface IDBServerFactory
    {
        IAdoAccessor CreatePostgreSqlAccessor(string connectionString);

        IMsAdoAccessor CreateMsSqlAccessor(string connectionString);
    }
}
