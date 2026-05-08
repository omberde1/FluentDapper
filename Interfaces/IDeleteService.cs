using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FluentDapper.Interfaces
{
    public interface IDeleteService
    {
        Task<int> HardAsync(string tableName, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null);
        Task<int> SoftAsync(string tableName, string setClause, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null);
        Task<int> SqlAsync(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null);
    }
}
