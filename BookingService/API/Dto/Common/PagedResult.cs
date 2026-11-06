namespace API.Dto.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages
);