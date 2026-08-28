using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CleanArchBlazorServer.Application.Interfaces;
using CleanArchBlazorServer.Domain.Entities;

namespace CleanArchBlazorServer.Infrastructure.ToDos;

public sealed class ToDoRepository(HttpClient httpClient) : IToDoRepository
{
    public async Task<IReadOnlyList<ToDoItem>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var items = await httpClient.GetFromJsonAsync<List<ToDoDto>>("todos", cancellationToken);
        return items?.Select(item => new ToDoItem(item.Id, item.Title, item.Completed)).ToList()
            ?? [];
    }

    private sealed record ToDoDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("completed")] bool Completed
    );
}
