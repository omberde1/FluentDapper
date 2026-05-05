using FluentDapper.Core;
using FluentDapper.Interfaces;
using FluentDapper.Operations;

namespace FluentDapper
{
    public class FluentDapperClient
    {
        public IInsertService Insert { get; }

        // later:
        // public UpdateService Update { get; }
        // public DeleteService Delete { get; }
        // public QueryService Query { get; }

        public FluentDapperClient(string connectionString)
        {
            var context = new DapperContext(connectionString);

            Insert = new InsertService(context);
            // Update = new UpdateService(context);
            // etc...
        }
    }
}