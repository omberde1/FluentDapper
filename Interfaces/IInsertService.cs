using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FluentDapper.Interfaces
{
    public interface IInsertService
    {
        Task<int> Single<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null);
        Task<TKey> Single<T, TKey>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct;
        Task<int> SingleWithIdentity<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null);
        Task Bulk<T>(string tableName, List<T> modelList, SqlConnection conn = null, SqlTransaction transaction = null);
    }
}
