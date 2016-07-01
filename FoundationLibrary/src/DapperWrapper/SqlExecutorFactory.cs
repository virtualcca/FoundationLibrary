using System;
using System.ComponentModel;
using System.Data.SqlClient;

namespace DapperWrapper
{
    public class SqlExecutorFactory : IDbExecutorFactory
    {
        readonly string _connectionString;

        public SqlExecutorFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;
        }

        public IDbExecutor CreateExecutor()
        {
            var dbConnection = new SqlConnection(_connectionString);
            return new SqlExecutor(dbConnection);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IDbExecutor CreateExecutor(string connectionString)
        {
            var dbConnection = new SqlConnection(connectionString);
            return new SqlExecutor(dbConnection);
        }
    }
}
