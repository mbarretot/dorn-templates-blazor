namespace CleanArchBlazorServer.Web.Features.ToDo;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task CreateAsync(string title, CancellationToken cancellationToken = default);

    Task SetCompletedAsync(
        int id,
        bool isCompleted,
        CancellationToken cancellationToken = default
    );

    Task UpdateTitleAsync(int id, string title, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
