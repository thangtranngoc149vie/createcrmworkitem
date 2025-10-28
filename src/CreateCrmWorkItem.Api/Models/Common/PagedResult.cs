using System.Collections.Generic;

namespace CreateCrmWorkItem.Api.Models.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
