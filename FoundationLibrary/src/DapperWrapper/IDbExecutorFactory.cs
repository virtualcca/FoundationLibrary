namespace DapperWrapper
{
    public interface IDbExecutorFactory
    {
        IDbExecutor CreateExecutor(string connectionString);
    }
}
