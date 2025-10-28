using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CreateCrmWorkItem.Api.Services;

public interface IRbacService
{
    Task EnsureAnyPermissionAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
}
