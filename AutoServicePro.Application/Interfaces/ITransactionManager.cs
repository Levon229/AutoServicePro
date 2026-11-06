namespace AutoServicePro.Application.Interfaces;

public interface ITransactionManager
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
}