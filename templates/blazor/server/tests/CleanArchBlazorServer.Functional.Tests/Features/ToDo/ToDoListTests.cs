using Bunit;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
#if (IncludeCleanArchitecture)
using CleanArchBlazorServer.Application.Interfaces;
using CleanArchBlazorServer.Domain.Entities;
#else
using CleanArchBlazorServer.Web.Features.ToDo;
#endif

namespace CleanArchBlazorServer.Functional.Tests.Features.ToDo;

public sealed class ToDoListTests : UiTestContext
{
    [Fact]
    public void ToDoList_RendersEachItemTitle()
    {
        Services.AddSingleton<IToDoRepository>(
            new FakeToDoRepository(
                new ToDoItem(1, "Buy milk", false),
                new ToDoItem(2, "Walk the dog", true)
            )
        );

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        Assert.Contains("Buy milk", cut.Markup);
        Assert.Contains("Walk the dog", cut.Markup);
    }

    [Fact]
    public void ToDoList_RendersEmptyState_WhenRepositoryHasNoItems()
    {
        Services.AddSingleton<IToDoRepository>(new FakeToDoRepository());

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        Assert.Contains("No to-dos found.", cut.Markup);
    }

    [Fact]
    public void ToDoList_RendersErrorAlert_WhenRepositoryThrows()
    {
        Services.AddSingleton<IToDoRepository>(new ThrowingToDoRepository());

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        Assert.Contains("Could not load to-dos.", cut.Markup);
    }

    [Fact]
    public void ToDoList_PassesCancellableToken_ToGetAllAsync()
    {
        var repository = new CapturingToDoRepository();
        Services.AddSingleton<IToDoRepository>(repository);

        Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        Assert.True(repository.CapturedToken.CanBeCanceled);
    }

    private sealed class FakeToDoRepository(params ToDoItem[] items) : IToDoRepository
    {
        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<ToDoItem>>(items);
    }

    private sealed class CapturingToDoRepository : IToDoRepository
    {
        public CancellationToken CapturedToken { get; private set; }

        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            CapturedToken = cancellationToken;
            return Task.FromResult<IReadOnlyList<ToDoItem>>([]);
        }
    }

    private sealed class ThrowingToDoRepository : IToDoRepository
    {
        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => throw new HttpRequestException("Simulated network failure.");
    }
}
