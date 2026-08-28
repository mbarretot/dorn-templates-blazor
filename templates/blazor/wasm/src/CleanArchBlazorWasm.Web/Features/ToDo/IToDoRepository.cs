namespace CleanArchBlazorWasm.Web.Features.ToDo;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
