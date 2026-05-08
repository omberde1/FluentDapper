using FluentDapper.Core;
using FluentDapper.Interfaces;
using Dapper;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FluentDapper.Operations
{
    internal class DeleteService : IDeleteService
    {
        private readonly DapperContext _context;
        public DeleteService(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permanently deletes records from the specified table.
        /// </summary>
        /// <param name="tableName">Target table name.</param>
        /// <param name="whereClause">SQL WHERE condition to filter records.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// This performs a physical delete. Use with caution.
        /// </remarks>
        public async Task<int> HardAsync(string tableName, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(whereClause)) throw new ArgumentException("WHERE clause is required for update.");

            var sql = $"DELETE FROM {tableName} WHERE {whereClause}";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Performs a soft delete by updating records (e.g., setting IsDeleted = 1).
        /// </summary>
        /// <param name="tableName">Target table name.</param>
        /// <param name="setClause">SET clause defining soft delete logic.</param>
        /// <param name="whereClause">SQL WHERE condition to filter records.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Commonly used to mark records as deleted instead of removing them permanently.
        /// </remarks>
        public async Task<int> SoftAsync(string tableName, string setClause, string whereClause, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(whereClause)) throw new ArgumentException("WHERE clause is required for update.");

            var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Executes a custom DELETE or data modification SQL query.
        /// </summary>
        /// <param name="sql">Full SQL query.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Use this method for complex delete operations or custom logic.
        /// </remarks>
        public async Task<int> SqlAsync(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.ExecuteAsync(sql, param, transaction).ConfigureAwait(false);
            }, connToUse);
        }
    }
}