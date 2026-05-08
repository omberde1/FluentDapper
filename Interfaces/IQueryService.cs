using Dapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FluentDapper.Interfaces
{
    public interface IQueryService
    {
        Task<List<T>> ListAsync<T>(string tableName, string whereClause = "", string orderBy = "", object param = null);
        Task<List<TResult>> ListByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, string orderBy = "");
        Task<List<T>> ListSqlAsync<T>(string sql, object param = null);

        Task<T> SingleAsync<T>(string tableName, string whereClause = "", string orderBy = "", object param = null);
        Task<TResult> SingleByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, string orderBy = "");
        Task<T> SingleSqlAsync<T>(string sql, object param = null);

        Task<List<T>> ColumnSqlAsync<T>(string sql, object param = null);
        Task<T> ValueSqlAsync<T>(string sql, object param = null);

        Task<List<T>> PagedListAsync<T>(string tableName, int pageNumber = 1, int pageSize = 10, string whereClause = "", string orderBy = "", object param = null);
        Task<List<TResult>> PageListByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, int pageNumber = 1, int pageSize = 10, string orderBy = "");
        Task<List<T>> PageListSqlAsync<T>(string sql, int pageNumber = 1, int pageSize = 10, object param = null);

        Task<List<T>> SPListAsync<T>(string storedProcedureName, object param = null);
        Task<T> SPSingleAsync<T>(string storedProcedureName, object param = null);

        Task<T> SPMultipleSetsAsync<T>(string storedProcedureName, object param, Func<SqlMapper.GridReader, Task<T>> mapFunc);
    }
}