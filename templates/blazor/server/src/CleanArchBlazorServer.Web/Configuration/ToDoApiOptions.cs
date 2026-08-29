namespace CleanArchBlazorServer.Web.Configuration;

public sealed class ToDoApiOptions
{
    public const string SectionName = "ToDoApi";

    public string BaseAddress { get; set; } = "https://jsonplaceholder.typicode.com/";
}
