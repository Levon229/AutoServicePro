using AutoServicePro.Application.Interfaces;
using AutoServicePro.Infrastructure.Data;

namespace AutoServicePro.Infrastructure.Transactions;

public sealed class EfTransactionManager(AppDbContext context) : ITransactionManager
{
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var result = await operation();
        await transaction.CommitAsync();
        return result;
    }
}