using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FluentDapper.Interfaces
{
    public interface IUpdateService
    {
        Task<int> EntityAsync<T>(string tableName, T model, string whereClause, SqlConnection conn = null, SqlTransaction transaction = null);

        Task<int> SetAsync(string tableName, string setClause, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null);

        Task<int> SqlAsync(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null);
    }
}