using Dapper;
using FluentDapper.Core;
using FluentDapper.Interfaces;
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
