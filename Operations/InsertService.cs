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

        public Task<int> EntityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            return EntityAsync<T, int>(tableName, model, conn, transaction);
        }
        public async Task<TKey> EntityAsync<T, TKey>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct
        {
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

        public async Task<int> EntityWithIdentityAsync<T>(string tableName, T model, SqlConnection conn = null, SqlTransaction transaction = null)
        {
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

        public async Task BulkAsync<T>(string tableName, List<T> modelList, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            if (modelList == null || modelList.Count == 0) return;

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

        public Task<int> SqlAsync<T>(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null)
        {
            return SqlAsync<T, int>(sql, param, conn, transaction);
        }
        public async Task<TKey> SqlAsync<T, TKey>(string sql, object param = null, SqlConnection conn = null, SqlTransaction transaction = null) where TKey : struct
        {
            sql = $"{sql} SELECT SCOPE_IDENTITY();";

            var connToUse = transaction?.Connection ?? conn;
            return await _context.WithConnectionAsync(async c =>
            {
                return await c.QuerySingleAsync<TKey>(sql, param, transaction).ConfigureAwait(false);
            }, connToUse);
        }
    }
}