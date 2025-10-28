using System;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Services;

public interface ITemplateService
{
    Task<TemplateSuggestResponse?> SuggestAsync(Guid orgId, string type, Guid? categoryId, Guid? subcategoryId, string? domain, CancellationToken cancellationToken);
}
