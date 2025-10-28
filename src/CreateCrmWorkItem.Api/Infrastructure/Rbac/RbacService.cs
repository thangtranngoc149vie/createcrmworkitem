using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Exceptions;
using CreateCrmWorkItem.Api.Repositories;
using CreateCrmWorkItem.Api.Services;

namespace CreateCrmWorkItem.Api.Infrastructure.Rbac;

public class RbacService : IRbacService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RbacService> _logger;

    public RbacService(IRoleRepository roleRepository, ILogger<RbacService> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task EnsureAnyPermissionAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var permissionList = permissions.Distinct().ToArray();
        if (permissionList.Length == 0)
        {
            return;
        }

        var hasPermission = await _roleRepository.HasAnyPermissionAsync(userId, permissionList, cancellationToken);
        if (!hasPermission)
        {
            _logger.LogWarning("User {UserId} missing permissions {Permissions}", userId, permissionList);
            throw new UnauthorizedException("RBAC denied");
        }
    }
}
