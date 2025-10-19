using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;
public interface IApplicationDbContext
{
    DbSet<Terminal> Terminals { get; }
    DbSet<Operator> Operators { get; }
    DbSet<OperatorTerminal> OperatorTerminalSessions { get; }
    DbSet<Job> Jobs { get; }
    DbSet<JobInProgress> JobsInProgress { get; }
    DbSet<JobItem> JobItems { get; }
    DbSet<ProductStock> ProductStocks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
