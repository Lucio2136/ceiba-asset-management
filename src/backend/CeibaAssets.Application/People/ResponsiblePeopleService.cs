using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.People;

public sealed record ResponsiblePersonModel(
    Guid Id,
    string EmployeeNumber,
    string DisplayName,
    string Email,
    string? Department,
    bool IsActive);

public sealed record ResponsiblePersonQuery(
    string? Search = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 25);

public interface IResponsiblePeopleService
{
    Task<PagedModel<ResponsiblePersonModel>> ListAsync(ResponsiblePersonQuery query, CancellationToken cancellationToken);

    Task<Result<ResponsiblePersonModel>> GetAsync(Guid id, CancellationToken cancellationToken);
}

internal sealed class ResponsiblePeopleService(IApplicationDbContext db) : IResponsiblePeopleService
{
    public async Task<PagedModel<ResponsiblePersonModel>> ListAsync(
        ResponsiblePersonQuery query,
        CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var people = db.ResponsiblePeople.AsNoTracking();

        if (!query.IncludeInactive)
        {
            people = people.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = SearchPattern.Contains(query.Search);
            people = people.Where(p =>
                EF.Functions.Like(p.DisplayName, pattern)
                || EF.Functions.Like(p.Email, pattern)
                || EF.Functions.Like(p.EmployeeNumber, pattern)
                || (p.Department != null && EF.Functions.Like(p.Department, pattern)));
        }

        var total = await people.CountAsync(cancellationToken);
        var items = await people
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ResponsiblePersonModel(p.Id, p.EmployeeNumber, p.DisplayName, p.Email, p.Department, p.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedModel<ResponsiblePersonModel>(items, total, page, pageSize);
    }

    public async Task<Result<ResponsiblePersonModel>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var person = await db.ResponsiblePeople
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ResponsiblePersonModel(p.Id, p.EmployeeNumber, p.DisplayName, p.Email, p.Department, p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return person is null
            ? Result.NotFound<ResponsiblePersonModel>("El responsable no existe.")
            : Result.Success(person);
    }
}
