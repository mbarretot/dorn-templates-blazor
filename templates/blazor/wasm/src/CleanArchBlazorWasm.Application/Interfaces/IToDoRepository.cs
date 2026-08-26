using CleanArchBlazorWasm.Domain.Entities;

namespace CleanArchBlazorWasm.Application.Interfaces;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
