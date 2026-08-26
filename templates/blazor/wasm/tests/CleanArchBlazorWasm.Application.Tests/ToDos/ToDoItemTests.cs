namespace CleanArchBlazorWasm.Application.Tests.ToDos;

public sealed class ToDoItemTests
{
    [Fact]
    public void Equals_ReturnsTrue_ForSameValues()
    {
        var first = new ToDoItem(1, "Buy milk", false);
        var second = new ToDoItem(1, "Buy milk", false);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenCompletionDiffers()
    {
        var pending = new ToDoItem(1, "Buy milk", false);
        var completed = pending with { IsCompleted = true };

        Assert.NotEqual(pending, completed);
    }
}
