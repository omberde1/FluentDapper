using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FluentDapper.Interfaces
{
    public interface IInsertService
    {
        Task<int> EntityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null);
        Task<TKey> EntityAsync<T, TKey>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct;

        Task<int> EntityWithIdentityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null);

        Task BulkAsync<T>(string tableName, List<T> modelList, SqlConnection conn = null, SqlTransaction transaction = null);

        Task<int> SqlAsync<T>(string tableName, object param, SqlConnection conn = null, SqlTransaction transaction = null);
        Task<TKey> SqlAsync<T, TKey>(string tableName, object param, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct;
    }
}
