using FluentDapper.Core;
using FluentDapper.Interfaces;
using FluentDapper.Operations;

namespace FluentDapper
{
    /// <summary>
    /// Entry point for FluentDapper.
    /// Provides access to Insert, Update, Delete, and Query operations
    /// using a simplified and fluent database abstraction over Dapper.
    /// </summary>
    /// <remarks>
    /// Create a single instance of this class and reuse it throughout your application.
    /// It manages internal connection handling via DapperContext.
    /// </remarks>
    public class FluentDapper
    {
        /// <summary>
        /// Provides insert operations for database entities.
        /// </summary>
        public IInsertService Insert { get; }
        /// <summary>
        /// Provides update operations for database entities.
        /// </summary>
        public IUpdateService Update { get; }
        /// <summary>
        /// Provides delete operations for database entities.
        /// </summary>
        public IDeleteService Delete { get; }
        /// <summary>
        /// Provides read/query operations for retrieving data.
        /// </summary>
        public IQueryService Query { get; }

        /// <summary>
        /// Initializes a new instance of FluentDapper using the provided SQL connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public FluentDapper(string connectionString)
        {
            var context = new DapperContext(connectionString);

            Insert = new InsertService(context);
            Update = new UpdateService(context);
            Delete = new DeleteService(context);
            Query = new QueryService(context);
        }
    }
}