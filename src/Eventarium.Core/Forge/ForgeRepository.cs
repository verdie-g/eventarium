namespace Eventarium.Core.Forge;

/// <summary>
/// Identifies a repository by its owner and repository name.
/// </summary>
/// <param name="Owner">The user or organization that owns the repository.</param>
/// <param name="Name">The repository name.</param>
public sealed record ForgeRepository(string Owner, string Name)
{
    /// <summary>
    /// Gets the repository identifier in <c>owner/name</c> form.
    /// </summary>
    public string FullName => $"{Owner}/{Name}";

    /// <summary>
    /// Gets the built-in repositories used when no repository list is configured.
    /// </summary>
    public static IReadOnlyList<ForgeRepository> Defaults { get; } =
    [
        new("microsoft", "TypeScript"),
        new("python", "cpython"),
        new("dotnet", "runtime"),
        new("openjdk", "jdk"),
        new("llvm", "llvm-project"),
        new("golang", "go"),
        new("rust-lang", "rust"),
        new("JetBrains", "kotlin")
    ];
}
