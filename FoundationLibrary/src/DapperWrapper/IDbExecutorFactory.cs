namespace DapperWrapper
{
    public interface IDbExecutorFactory
    {
        IDbExecutor CreateExecutor();

        IDbExecutor CreateExecutor(string connectionString);
    }
}
