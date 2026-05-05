using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FluentDapper.Core
{
    internal class DapperContext
    {
        public readonly string _connectionString;

        public DapperContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> func, SqlConnection existingConn = null)
        {
            if (existingConn != null)
                return await func(existingConn).ConfigureAwait(false);

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync().ConfigureAwait(false);
                return await func(conn).ConfigureAwait(false);
            }
        }
    }
}