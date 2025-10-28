using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CreateCrmWorkItem.Api.Infrastructure.Database;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
