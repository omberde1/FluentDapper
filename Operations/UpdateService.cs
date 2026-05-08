using FluentDapper.Core;
using FluentDapper.Interfaces;
using Dapper;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FluentDapper.Operations
{
    internal class UpdateService : IUpdateService
    {
        private readonly DapperContext _context;
        public UpdateService(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Updates records in the specified table using values from the provided model.
        /// Only non-null properties are included in the SET clause.
        /// </summary>
        /// <typeparam name="T">Type of the model containing updated values.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="model">Object containing values to update.</param>
        /// <param name="whereClause">SQL WHERE condition to filter records.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Ensure the WHERE clause is properly defined to avoid updating unintended records.
        /// </remarks>
        public async Task<int> EntityAsync<T>(string tableName, T model, string whereClause, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(whereClause)) throw new ArgumentException("WHERE clause is required for update.");
            if (model == null) throw new ArgumentNullException(nameof(model));

            var props = DapperHelpers.GetNonNullProperties(model)
                .Where(p => !string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!props.Any())
                throw new InvalidOperationException("No updatable properties found in the model.");

            var setClause = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));

            // Using DynamicParameters just like Insert
            var paramObject = new DynamicParameters();
            foreach (var prop in props)
            {
                var value = DapperHelpers.EnsureSafeValue(prop.PropertyType, prop.GetValue(model));
                paramObject.Add("@" + prop.Name, value);
            }

            var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.ExecuteAsync(sql, paramObject, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Updates records in the specified table using a custom SET clause.
        /// </summary>
        /// <param name="tableName">Target table name.</param>
        /// <param name="setClause">Custom SET clause (e.g., "Name = @Name").</param>
        /// <param name="whereClause">SQL WHERE condition to filter records.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Use this method when you need full control over the update statement.
        /// </remarks>
        public async Task<int> SetAsync(string tableName, string setClause, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(setClause)) throw new ArgumentException("SET clause is required for update.");
            if (string.IsNullOrWhiteSpace(whereClause)) throw new ArgumentException("WHERE clause is required for update.");

            var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                var rowsAffected = await c.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
                return rowsAffected;
            }, connToUse);
        }

        /// <summary>
        /// Executes a custom UPDATE SQL query.
        /// </summary>
        /// <param name="sql">Full SQL update query.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Use this method for complex or fully custom update operations.
        /// </remarks>
        public async Task<int> SqlAsync(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                var rowsAffected = await c.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
                return rowsAffected;
            }, connToUse);
        }
    }
}