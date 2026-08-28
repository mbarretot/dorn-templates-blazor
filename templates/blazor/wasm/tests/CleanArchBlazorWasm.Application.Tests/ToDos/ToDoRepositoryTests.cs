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

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequestUri = request.RequestUri;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
