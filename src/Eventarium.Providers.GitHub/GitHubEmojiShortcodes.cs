using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Eventarium.Providers.GitHub;

internal static partial class GitHubEmojiShortcodes
{
    private const string AliasesResourceName = "Eventarium.Providers.GitHub.GitHubEmojiAliases.json";
    private static readonly FrozenDictionary<string, string> Aliases = LoadAliases();

    public static string Emojify(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.IndexOf(':') < 0)
        {
            return value;
        }

        return Shortcode().Replace(value, static match =>
            Aliases.TryGetValue(match.Groups[1].Value, out string? emoji)
                ? emoji
                : match.Value);
    }

    private static FrozenDictionary<string, string> LoadAliases()
    {
        using Stream stream = typeof(GitHubEmojiShortcodes).Assembly
            .GetManifestResourceStream(AliasesResourceName) ??
            throw new InvalidOperationException($"Missing embedded resource {AliasesResourceName}.");
        Dictionary<string, string>? aliases = JsonSerializer.Deserialize(
            stream,
            GitHubEmojiJsonSerializerContext.Default.DictionaryStringString);

        return aliases?.ToFrozenDictionary(StringComparer.Ordinal) ??
            throw new InvalidOperationException("The GitHub emoji alias data is invalid.");
    }

    [GeneratedRegex(":([A-Za-z0-9_+-]+):", RegexOptions.CultureInvariant)]
    private static partial Regex Shortcode();
}

[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class GitHubEmojiJsonSerializerContext : JsonSerializerContext;
