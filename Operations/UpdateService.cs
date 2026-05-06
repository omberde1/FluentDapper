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
