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

    public async Task CreateAsync(string title, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "todos",
            new { title, completed = false },
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task SetCompletedAsync(
        int id,
        bool isCompleted,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PatchAsJsonAsync(
            $"todos/{id}",
            new { completed = isCompleted },
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTitleAsync(
        int id,
        string title,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PatchAsJsonAsync(
            $"todos/{id}",
            new { title },
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"todos/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed record ToDoDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("completed")] bool Completed
    );
}
