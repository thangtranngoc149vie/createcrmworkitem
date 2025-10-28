using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CreateCrmWorkItem.Api.Repositories;

public interface IRoleRepository
{
    Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken);
}
