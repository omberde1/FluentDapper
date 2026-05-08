using FluentDapper.Core;
using FluentDapper.Interfaces;
using FluentDapper.Operations;

namespace FluentDapper
{
    public class FluentDapperClient
    {
        public IInsertService Insert { get; }
        public IUpdateService Update { get; }
        public IDeleteService Delete { get; }
        // public IQueryService Query { get; }

        public FluentDapperClient(string connectionString)
        {
            var context = new DapperContext(connectionString);

            Insert = new InsertService(context);
            Update = new UpdateService(context);
            Delete = new DeleteService(context);
        }
    }
}