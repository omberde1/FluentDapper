using FluentDapper.Core;
using FluentDapper.Interfaces;
using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FluentDapper.Operations
{
    internal class QueryService : IQueryService
    {
        private readonly DapperContext _context;
        public QueryService(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of records from a table with optional WHERE and ORDER BY clauses.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="whereClause">Optional WHERE condition (without 'WHERE').</param>
        /// <param name="orderBy">Optional ORDER BY clause (without 'ORDER BY').</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A list of mapped records.</returns>
        public async Task<List<T>> ListAsync<T>(string tableName, string whereClause = "", string orderBy = "", object param = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");

            var sql = $"SELECT * FROM {tableName} {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")}";

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(sql, param).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Retrieves a list of records by automatically generating a WHERE clause from a filter model.
        /// Only non-null properties are included in filtering.
        /// </summary>
        /// <typeparam name="TFilter">The filter model type.</typeparam>
        /// <typeparam name="TResult">The result mapping type.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="filterModel">Object containing filter values.</param>
        /// <param name="orderBy">Optional ORDER BY clause.</param>
        /// <returns>A filtered list of mapped records.</returns>
        public async Task<List<TResult>> ListByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, string orderBy = "")
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");

            var props = DapperHelpers.GetNonNullProperties(filterModel).ToList();

            var whereParts = new List<string>();
            var paramObject = new DynamicParameters();

            foreach (var prop in props)
            {
                whereParts.Add($"{prop.Name} = @{prop.Name}");
                var value = DapperHelpers.EnsureSafeValue(prop.PropertyType, prop.GetValue(filterModel));
                paramObject.Add("@" + prop.Name, value);
            }

            // Default to no filter if no non-null properties exist
            var whereClause = whereParts.Any() ? string.Join(" AND ", whereParts) : "1=1";

            var sql = $"SELECT * FROM {tableName} {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")}";

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<TResult>(sql, paramObject).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Executes a raw SQL query and returns a list of results.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<T>> ListSqlAsync<T>(string sql, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(sql, param).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Retrieves a single record from a table using optional WHERE and ORDER BY clauses.
        /// Returns the first match or default if none found.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="whereClause">Optional WHERE condition.</param>
        /// <param name="orderBy">Optional ORDER BY clause.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A single mapped record or default.</returns>
        public async Task<T> SingleAsync<T>(string tableName, string whereClause = "", string orderBy = "", object param = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");

            var sql = $"SELECT TOP 1 * FROM {tableName} {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")}";

            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QueryFirstOrDefaultAsync<T>(sql, param).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Retrieves a single record by automatically generating a WHERE clause from a filter model.
        /// Only non-null properties are used for filtering.
        /// </summary>
        /// <typeparam name="TFilter">The filter model type.</typeparam>
        /// <typeparam name="TResult">The result mapping type.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="filterModel">Object containing filter values.</param>
        /// <param name="orderBy">Optional ORDER BY clause.</param>
        /// <returns>A single mapped record or default.</returns>
        public async Task<TResult> SingleByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, string orderBy = "")
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");

            var props = DapperHelpers.GetNonNullProperties(filterModel).ToList();

            var whereParts = new List<string>();
            var paramObject = new DynamicParameters();

            foreach (var prop in props)
            {
                whereParts.Add($"{prop.Name} = @{prop.Name}");
                var value = DapperHelpers.EnsureSafeValue(prop.PropertyType, prop.GetValue(filterModel));
                paramObject.Add("@" + prop.Name, value);
            }

            var whereClause = whereParts.Any() ? string.Join(" AND ", whereParts) : "1=1";
            var sql = $"SELECT TOP 1 * FROM {tableName} {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")}";

            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QueryFirstOrDefaultAsync<TResult>(sql, paramObject).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Executes a raw SQL query and returns a single result.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A single mapped result or default.</returns>
        public async Task<T> SingleSqlAsync<T>(string sql, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");

            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QueryFirstOrDefaultAsync<T>(sql, param).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Executes a SQL query that returns a single column and maps it to a list.
        /// </summary>
        /// <typeparam name="T">The column type.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A list of values from a single column.</returns>
        public async Task<List<T>> ColumnSqlAsync<T>(string sql, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(sql, param).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Executes a SQL query that returns a single value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A single value or default.</returns>
        public async Task<T> ValueSqlAsync<T>(string sql, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("TABLE name cannot be null or empty.");

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryFirstOrDefaultAsync<T>(sql, param).ConfigureAwait(false);
                return result;
            });
        }

        /// <summary>
        /// Retrieves a paginated list of records from a table using OFFSET-FETCH.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="pageNumber">The page number (starting from 1).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="whereClause">Optional WHERE condition.</param>
        /// <param name="orderBy">ORDER BY clause (required for pagination).</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A paginated list of mapped records.</returns>
        public async Task<List<T>> PagedListAsync<T>(string tableName, int pageNumber = 1, int pageSize = 10, string whereClause = "", string orderBy = "", object param = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(orderBy)) throw new ArgumentException("ORDER BY clause is required for pagination.");

            int offset = (pageNumber - 1) * pageSize;

            var sql = $@"
            SELECT * FROM {tableName} 
            {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} 
            {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")} 
            OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(sql, param).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Retrieves a paginated list of records using a filter model.
        /// Supports automatic WHERE clause generation and LIKE for string fields.
        /// </summary>
        /// <typeparam name="TFilter">The filter model type.</typeparam>
        /// <typeparam name="TResult">The result mapping type.</typeparam>
        /// <param name="tableName">The database table name.</param>
        /// <param name="filterModel">Object containing filter values.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="orderBy">ORDER BY clause (required).</param>
        /// <returns>A filtered and paginated list of mapped results.</returns>
        public async Task<List<TResult>> PageListByWhereModelAsync<TFilter, TResult>(string tableName, TFilter filterModel, int pageNumber = 1, int pageSize = 10, string orderBy = "")
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(orderBy)) throw new ArgumentException("ORDER BY clause is required for pagination.");

            var props = DapperHelpers.GetNonNullProperties(filterModel).ToList();

            var whereParts = new List<string>();
            var paramObject = new DynamicParameters();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var value = prop.GetValue(filterModel);
                var safeValue = DapperHelpers.EnsureSafeValue(prop.PropertyType, value);

                if (prop.PropertyType == typeof(string))
                {
                    whereParts.Add($"{propName} LIKE @{propName}");
                    paramObject.Add("@" + propName, $"%{safeValue}%");
                }
                else
                {
                    whereParts.Add($"{propName} = @{propName}");
                    paramObject.Add("@" + propName, safeValue);
                }
            }

            var whereClause = whereParts.Any() ? string.Join(" AND ", whereParts) : "1=1";

            int offset = (pageNumber - 1) * pageSize;

            var sql = $@"
                SELECT * FROM {tableName}
                {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")} 
                {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"ORDER BY {orderBy}")} 
                OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY;
            ";

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<TResult>(sql, paramObject).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Executes a raw SQL query with pagination using OFFSET-FETCH.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="sql">The base SQL query (without pagination).</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>A paginated list of mapped results.</returns>
        public async Task<List<T>> PageListSqlAsync<T>(string sql, int pageNumber = 1, int pageSize = 10, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));

            int offset = (pageNumber - 1) * pageSize;

            // Append pagination and ordering to the provided SQL query
            var pagedSql = $@"
            {sql}
            OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(pagedSql, param).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Executes a stored procedure and returns a list of results.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="storedProcedureName">The stored procedure name.</param>
        /// <param name="param">Optional parameters for the stored procedure.</param>
        /// <returns>A list of mapped results.</returns>
        public async Task<List<T>> SPListAsync<T>(string storedProcedureName, object param = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName)) throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryAsync<T>(
                    storedProcedureName,
                    param,
                    commandType: CommandType.StoredProcedure
                ).ConfigureAwait(false);
                return result.ToList();
            });
        }

        /// <summary>
        /// Executes a stored procedure and returns a single result.
        /// </summary>
        /// <typeparam name="T">The type to map the result to.</typeparam>
        /// <param name="storedProcedureName">The stored procedure name.</param>
        /// <param name="param">Optional parameters for the stored procedure.</param>
        /// <returns>A single mapped result or default.</returns>
        public async Task<T> SPSingleAsync<T>(string storedProcedureName, object param = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName)) throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            return await _context.WithConnectionAsync(async c =>
            {
                var result = await c.QueryFirstOrDefaultAsync<T>(
                    storedProcedureName,
                    param,
                    commandType: CommandType.StoredProcedure
                ).ConfigureAwait(false);
                return result;
            });
        }

        /// <summary>
        /// Executes a stored procedure that returns multiple result sets.
        /// Allows custom mapping using a delegate function.
        /// </summary>
        /// <typeparam name="T">The final mapped result type.</typeparam>
        /// <param name="storedProcedureName">The stored procedure name.</param>
        /// <param name="param">Parameters for the stored procedure.</param>
        /// <param name="mapFunc">Function to map multiple result sets.</param>
        /// <returns>A custom mapped result.</returns>
        public async Task<T> SPMultipleSetsAsync<T>(string storedProcedureName, object param, Func<SqlMapper.GridReader, Task<T>> mapFunc)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName)) throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            return await _context.WithConnectionAsync(async c =>
            {
                using (var multi = await c.QueryMultipleAsync(
                    storedProcedureName,
                    param,
                    commandType: CommandType.StoredProcedure))
                {
                    return await mapFunc(multi).ConfigureAwait(false);
                }
            });
        }
    }
}