namespace CleanArchBlazorWasm.Web.Features.Playground;

public sealed record PlaygroundParameter(
    string Name,
    string Type,
    string Default,
    string Description
);
