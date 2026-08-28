using Bunit;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
#if (IncludeCleanArchitecture)
using CleanArchBlazorWasm.Application.Interfaces;
using CleanArchBlazorWasm.Domain.Entities;
#else
using CleanArchBlazorWasm.Web.Features.ToDo;
#endif

namespace CleanArchBlazorWasm.Functional.Tests.Features.ToDo;

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
            builder.OpenComponent<CleanArchBlazorWasm.Web.Features.ToDo.ToDoList>(0);
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
            builder.OpenComponent<CleanArchBlazorWasm.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        Assert.Contains("No to-dos found.", cut.Markup);
    }

    private sealed class FakeToDoRepository(params ToDoItem[] items) : IToDoRepository
    {
        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<ToDoItem>>(items);
    }
}
