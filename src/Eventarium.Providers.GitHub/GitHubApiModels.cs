using System.Text.Json.Serialization;

namespace Eventarium.Providers.GitHub;

public sealed record GitHubActivityEvent
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("actor")]
    public GitHubActor Actor { get; init; } = new();

    [JsonPropertyName("repo")]
    public GitHubEventRepository Repository { get; init; } = new();

    [JsonPropertyName("payload")]
    public GitHubEventPayload Payload { get; init; } = new();

    [JsonPropertyName("public")]
    public bool IsPublic { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}

public sealed record GitHubActor
{
    [JsonPropertyName("login")]
    public string Login { get; init; } = "";
}

public sealed record GitHubEventRepository
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
}

public sealed record GitHubEventPayload
{
    [JsonPropertyName("repository_id")]
    public long? RepositoryId { get; init; }

    [JsonPropertyName("push_id")]
    public long? PushId { get; init; }

    [JsonPropertyName("ref")]
    public string? Ref { get; init; }

    [JsonPropertyName("head")]
    public string? Head { get; init; }

    [JsonPropertyName("before")]
    public string? Before { get; init; }

    [JsonPropertyName("action")]
    public string? Action { get; init; }

    [JsonPropertyName("number")]
    public int? Number { get; init; }

    [JsonPropertyName("issue")]
    public GitHubIssue? Issue { get; init; }

    [JsonPropertyName("pull_request")]
    public GitHubPullRequest? PullRequest { get; init; }

    [JsonPropertyName("comment")]
    public GitHubComment? Comment { get; init; }

    [JsonPropertyName("review")]
    public GitHubPullRequestReview? Review { get; init; }
}

public sealed record GitHubIssue
{
    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = "";

    [JsonPropertyName("pull_request")]
    public GitHubIssuePullRequest? PullRequest { get; init; }
}

public sealed record GitHubIssuePullRequest;

public sealed record GitHubPullRequest
{
    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = "";
}

public sealed record GitHubComment
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("user")]
    public GitHubUser? User { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = "";

    [JsonPropertyName("in_reply_to_id")]
    public long? InReplyToId { get; init; }
}

public sealed record GitHubPullRequestReview
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("user")]
    public GitHubUser? User { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("state")]
    public string State { get; init; } = "";

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = "";
}

public sealed record GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; init; } = "";
}

[JsonSerializable(typeof(List<GitHubActivityEvent>))]
internal sealed partial class GitHubJsonSerializerContext : JsonSerializerContext;
