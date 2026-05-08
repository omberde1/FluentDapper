using FluentDapper.Core;
using FluentDapper.Interfaces;
using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FluentDapper.Operations
{
    internal class InsertService : IInsertService
    {
        private readonly DapperContext _context;
        public InsertService(DapperContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inserts a single entity into the specified table and returns the generated identity value (default: int).
        /// </summary>
        /// <typeparam name="T">Type of the model to insert.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="model">Object containing values to insert.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Generated identity value (int).</returns>
        public Task<int> EntityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            return EntityAsync<T, int>(tableName, model, conn, transaction);
        }

        /// <summary>
        /// Inserts a single entity into the specified table and returns the generated identity value.
        /// </summary>
        /// <typeparam name="T">Type of the model to insert.</typeparam>
        /// <typeparam name="TKey">Type of the identity key (e.g., int, long, decimal).</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="model">Object containing values to insert.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Generated identity value.</returns>
        public async Task<TKey> EntityAsync<T, TKey>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (model == null) throw new ArgumentNullException(nameof(model));

            var props = DapperHelpers.GetNonNullProperties(model)
                .Where(p => !string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var columns = string.Join(",", props.Select(p => p.Name));
            var values = string.Join(",", props.Select(p => "@" + p.Name));

            var paramObject = new DynamicParameters();
            foreach (var prop in props)
            {
                var value = DapperHelpers.EnsureSafeValue(prop.PropertyType, prop.GetValue(model));
                paramObject.Add("@" + prop.Name, value);
            }

            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values}); SELECT SCOPE_IDENTITY();";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QuerySingleAsync<TKey>(sql, paramObject, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Inserts a single entity into the specified table including identity column values (IDENTITY_INSERT ON).
        /// </summary>
        /// <typeparam name="T">Type of the model to insert.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="model">Object containing values including identity column.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Use this only when you need to manually insert identity values.
        /// </remarks>
        public async Task<int> EntityWithIdentityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (model == null) throw new ArgumentNullException(nameof(model));

            var props = DapperHelpers.GetCachedProperties(typeof(T)).ToList();

            if (!props.Any()) throw new Exception("No properties found in the model.");

            var columns = string.Join(", ", props.Select(p => p.Name));
            var values = string.Join(", ", props.Select(p => "@" + p.Name));

            var paramObject = new DynamicParameters();

            foreach (var prop in props)
            {
                var value = DapperHelpers.EnsureSafeValue(prop.PropertyType, prop.GetValue(model));
                paramObject.Add("@" + prop.Name, value);
            }

            var sql = $@"
                BEGIN TRY
                    SET IDENTITY_INSERT {tableName} ON;
                    INSERT INTO {tableName} ({columns})
                    VALUES ({values});
                    SET IDENTITY_INSERT {tableName} OFF;
                END TRY
                BEGIN CATCH
                    SET IDENTITY_INSERT {tableName} OFF;
                    THROW;
                END CATCH
            ";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.ExecuteAsync(sql, paramObject, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Inserts multiple records into the specified table in a single operation.
        /// </summary>
        /// <typeparam name="T">Type of the model to insert.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="modelList">List of objects to insert.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Uses Dapper's bulk execution for improved performance.
        /// </remarks>
        public async Task BulkAsync<T>(string tableName, List<T> modelList, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TABLE name cannot be null or empty.");
            if (modelList == null || modelList.Count == 0) throw new ArgumentNullException("ModelList cannot be NULL or EMPTY");

            var props = DapperHelpers.GetCachedProperties(typeof(T))
                .Where(p => !string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var columns = string.Join(",", props.Select(p => p.Name));
            var values = string.Join(",", props.Select(p => "@" + p.Name));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            var connToUse = transaction?.Connection ?? conn;
            await _context.WithConnectionAsync(async c =>
            {
                // Dapper automatically executes multiple inserts efficiently when given a list
                return await c.ExecuteAsync(sql, modelList, transaction).ConfigureAwait(false);
            }, connToUse);
        }

        /// <summary>
        /// Executes a custom INSERT SQL query and returns the generated identity value (default: int).
        /// </summary>
        /// <typeparam name="T">Type placeholder (not used, for API consistency).</typeparam>
        /// <param name="sql">Custom SQL INSERT query.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Generated identity value (int).</returns>
        public Task<int> SqlAsync<T>(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            return SqlAsync<T, int>(sql, param, conn, transaction);
        }

        /// <summary>
        /// Executes a custom INSERT SQL query and returns the generated identity value.
        /// </summary>
        /// <typeparam name="T">Type placeholder (not used, for API consistency).</typeparam>
        /// <typeparam name="TKey">Type of the identity key.</typeparam>
        /// <param name="sql">Custom SQL INSERT query.</param>
        /// <param name="param">Query parameters.</param>
        /// <param name="conn">Optional existing SQL connection.</param>
        /// <param name="transaction">Optional SQL transaction.</param>
        /// <returns>Generated identity value.</returns>
        /// <remarks>
        /// Automatically appends SELECT SCOPE_IDENTITY() to the query.
        /// </remarks>
        public async Task<TKey> SqlAsync<T, TKey>(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL cannot be null or empty.");
            sql = $"{sql} SELECT SCOPE_IDENTITY();";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QuerySingleAsync<TKey>(sql, param, transaction).ConfigureAwait(false);
            }, connToUse);
        }
    }
}