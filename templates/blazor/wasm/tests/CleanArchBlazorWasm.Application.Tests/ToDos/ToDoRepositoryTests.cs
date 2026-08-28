namespace CleanArchBlazorWasm.Application.Tests.ToDos;

public sealed class ToDoRepositoryTests
{
    private const string BaseAddress = "https://jsonplaceholder.typicode.com/";

    [Fact]
    public async Task GetAllAsync_MapsJsonPlaceholderResponseToToDoItems()
    {
        const string responseJson = """
            [
                { "id": 1, "title": "Buy milk", "completed": false },
                { "id": 2, "title": "Walk the dog", "completed": true }
            ]
            """;
        var repository = CreateRepository(responseJson, out _);

        var items = await repository.GetAllAsync();

        Assert.Equal(
            [new ToDoItem(1, "Buy milk", false), new ToDoItem(2, "Walk the dog", true)],
            items
        );
    }

    [Fact]
    public async Task GetAllAsync_RequestsTodosEndpoint()
    {
        var repository = CreateRepository("[]", out var handler);

        await repository.GetAllAsync();

        Assert.Equal(BaseAddress + "todos", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task CreateAsync_PostsToTodosEndpoint()
    {
        var repository = CreateRepository("{}", out var handler);

        await repository.CreateAsync("Buy milk");

        Assert.Equal(HttpMethod.Post, handler.LastRequestMethod);
        Assert.Equal(BaseAddress + "todos", handler.LastRequestUri?.ToString());
        Assert.Contains("Buy milk", handler.LastRequestBody);
    }

    [Fact]
    public async Task SetCompletedAsync_PatchesTodoByIdEndpoint()
    {
        var repository = CreateRepository("{}", out var handler);

        await repository.SetCompletedAsync(1, true);

        Assert.Equal(HttpMethod.Patch, handler.LastRequestMethod);
        Assert.Equal(BaseAddress + "todos/1", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task DeleteAsync_DeletesTodoByIdEndpoint()
    {
        var repository = CreateRepository("{}", out var handler);

        await repository.DeleteAsync(1);

        Assert.Equal(HttpMethod.Delete, handler.LastRequestMethod);
        Assert.Equal(BaseAddress + "todos/1", handler.LastRequestUri?.ToString());
    }

    private static ToDoRepository CreateRepository(
        string responseJson,
        out StubHttpMessageHandler handler
    )
    {
        handler = new StubHttpMessageHandler(responseJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) };
        return new ToDoRepository(httpClient);
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        public HttpMethod? LastRequestMethod { get; private set; }

        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequestUri = request.RequestUri;
            LastRequestMethod = request.Method;
            LastRequestBody =
                request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
            return response;
        }
    }
}
