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

    [Fact]
    public async Task ToDoList_AddsNewItem_WhenFormSubmittedWithTitle()
    {
        var repository = new RecordingToDoRepository();
        Services.AddSingleton<IToDoRepository>(repository);

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        // MudForm renders a hidden native submit button ahead of the visible "Add" button, so
        // button index [1] is the Add button (index [0] is the hidden submit button).
        await cut.InvokeAsync(() => cut.Find("input").Change("Buy milk"));
        await cut.InvokeAsync(() => cut.FindAll("button")[1].Click());

        Assert.Contains("Buy milk", cut.Markup);
        Assert.True(repository.CreateInvoked);
        Assert.Equal("Buy milk", repository.CreatedTitle);
    }

    [Fact]
    public async Task ToDoList_TogglesCompletion_WhenItemHasPositiveId()
    {
        var repository = new RecordingToDoRepository(new ToDoItem(1, "Buy milk", false));
        Services.AddSingleton<IToDoRepository>(repository);

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        // Button order in the rendered markup: [0] hidden submit, [1] Add, [2] item toggle,
        // [3] item edit, [4] item delete.
        await cut.InvokeAsync(() => cut.FindAll("button")[2].Click());

        Assert.True(repository.SetCompletedInvoked);
        Assert.Equal(1, repository.SetCompletedId);
        Assert.True(repository.SetCompletedValue);
    }

    [Fact]
    public async Task ToDoList_UpdatesItemTitle_WhenEditSubmitted()
    {
        var repository = new RecordingToDoRepository(new ToDoItem(1, "Buy milk", false));
        Services.AddSingleton<IToDoRepository>(repository);

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        // Button order in the rendered markup: [0] hidden submit, [1] Add, [2] item toggle,
        // [3] item edit (becomes Save once editing), [4] item delete (becomes Cancel).
        await cut.InvokeAsync(() => cut.FindAll("button")[3].Click());
        await cut.InvokeAsync(() => cut.FindAll("input")[1].Change("Buy oat milk"));
        await cut.InvokeAsync(() => cut.FindAll("button")[3].Click());

        Assert.Contains("Buy oat milk", cut.Markup);
        Assert.True(repository.UpdateTitleInvoked);
        Assert.Equal(1, repository.UpdatedId);
        Assert.Equal("Buy oat milk", repository.UpdatedTitle);
    }

    [Fact]
    public async Task ToDoList_RemovesItemFromMarkup_WhenDeleteClicked()
    {
        var repository = new RecordingToDoRepository(new ToDoItem(1, "Buy milk", false));
        Services.AddSingleton<IToDoRepository>(repository);

        var cut = Render(builder =>
        {
            builder.OpenComponent<CleanArchBlazorServer.Web.Features.ToDo.ToDoList>(0);
            builder.CloseComponent();
        });

        // Button order in the rendered markup: [0] hidden submit, [1] Add, [2] item toggle,
        // [3] item edit, [4] item delete.
        await cut.InvokeAsync(() => cut.FindAll("button")[4].Click());

        Assert.DoesNotContain("Buy milk", cut.Markup);
        Assert.True(repository.DeleteInvoked);
        Assert.Equal(1, repository.DeletedId);
    }

    private sealed class FakeToDoRepository(params ToDoItem[] items) : IToDoRepository
    {
        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<ToDoItem>>(items);

        public Task CreateAsync(string title, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SetCompletedAsync(
            int id,
            bool isCompleted,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task UpdateTitleAsync(
            int id,
            string title,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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

        public Task CreateAsync(string title, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SetCompletedAsync(
            int id,
            bool isCompleted,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task UpdateTitleAsync(
            int id,
            string title,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingToDoRepository : IToDoRepository
    {
        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => throw new HttpRequestException("Simulated network failure.");

        public Task CreateAsync(string title, CancellationToken cancellationToken = default) =>
            throw new HttpRequestException("Simulated network failure.");

        public Task SetCompletedAsync(
            int id,
            bool isCompleted,
            CancellationToken cancellationToken = default
        ) => throw new HttpRequestException("Simulated network failure.");

        public Task UpdateTitleAsync(
            int id,
            string title,
            CancellationToken cancellationToken = default
        ) => throw new HttpRequestException("Simulated network failure.");

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
            throw new HttpRequestException("Simulated network failure.");
    }

    private sealed class RecordingToDoRepository(params ToDoItem[] items) : IToDoRepository
    {
        public bool CreateInvoked { get; private set; }
        public string? CreatedTitle { get; private set; }
        public bool SetCompletedInvoked { get; private set; }
        public int? SetCompletedId { get; private set; }
        public bool? SetCompletedValue { get; private set; }
        public bool UpdateTitleInvoked { get; private set; }
        public int? UpdatedId { get; private set; }
        public string? UpdatedTitle { get; private set; }
        public bool DeleteInvoked { get; private set; }
        public int? DeletedId { get; private set; }

        public Task<IReadOnlyList<ToDoItem>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<ToDoItem>>(items);

        public Task CreateAsync(string title, CancellationToken cancellationToken = default)
        {
            CreateInvoked = true;
            CreatedTitle = title;
            return Task.CompletedTask;
        }

        public Task SetCompletedAsync(
            int id,
            bool isCompleted,
            CancellationToken cancellationToken = default
        )
        {
            SetCompletedInvoked = true;
            SetCompletedId = id;
            SetCompletedValue = isCompleted;
            return Task.CompletedTask;
        }

        public Task UpdateTitleAsync(
            int id,
            string title,
            CancellationToken cancellationToken = default
        )
        {
            UpdateTitleInvoked = true;
            UpdatedId = id;
            UpdatedTitle = title;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DeleteInvoked = true;
            DeletedId = id;
            return Task.CompletedTask;
        }
    }
}
