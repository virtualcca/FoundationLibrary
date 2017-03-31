using System.Data.SqlClient;

namespace DapperWrapper
{
    public class SqlExecutorFactory : IDbExecutorFactory
    {
        public IDbExecutor CreateExecutor(string connectionString)
        {
            var dbConnection = new SqlConnection(connectionString);
            return new SqlExecutor(dbConnection);
        }
    }
}
