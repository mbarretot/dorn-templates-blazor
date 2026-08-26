using CleanArchBlazorServer.Domain.Entities;

namespace CleanArchBlazorServer.Application.Interfaces;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
