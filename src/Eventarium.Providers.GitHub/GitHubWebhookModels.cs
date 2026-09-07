using System.Text.Json.Serialization;

namespace Eventarium.Providers.GitHub;

/// <summary>
/// Models GitHub's official <c>webhook-push</c> schema.
/// </summary>
internal sealed record GitHubPushWebhookPayload
{
    public string Ref { get; init; } = "";

    public IReadOnlyList<GitHubWebhookCommit> Commits { get; init; } = [];

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }

    public GitHubWebhookCommitter Pusher { get; init; } = new();
}

internal sealed record GitHubWebhookCommit
{
    public string Id { get; init; } = "";

    public string Message { get; init; } = "";

    public DateTimeOffset? Timestamp { get; init; }

    public string Url { get; init; } = "";

    public GitHubWebhookCommitter Author { get; init; } = new();
}

internal sealed record GitHubWebhookCommitter
{
    public string Name { get; init; } = "";

    public string? Username { get; init; }
}

/// <summary>
/// Models the common fields in GitHub's <c>webhook-issues-opened</c>,
/// <c>webhook-issues-closed</c>, and <c>webhook-issues-reopened</c> schemas.
/// </summary>
internal sealed record GitHubIssuesWebhookPayload
{
    public string Action { get; init; } = "";

    public GitHubWebhookIssue Issue { get; init; } = new();

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }
}

/// <summary>
/// Models GitHub's official <c>webhook-issue-comment-created</c> schema.
/// </summary>
internal sealed record GitHubIssueCommentWebhookPayload
{
    public string Action { get; init; } = "";

    public GitHubWebhookIssue Issue { get; init; } = new();

    public GitHubWebhookComment Comment { get; init; } = new();

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }
}

/// <summary>
/// Models the common fields in GitHub's <c>webhook-pull-request-opened</c>,
/// <c>webhook-pull-request-closed</c>, and <c>webhook-pull-request-reopened</c> schemas.
/// </summary>
internal sealed record GitHubPullRequestWebhookPayload
{
    public string Action { get; init; } = "";

    public int Number { get; init; }

    public GitHubWebhookPullRequest PullRequest { get; init; } = new();

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }
}

/// <summary>
/// Models the common fields in GitHub's <c>webhook-pull-request-review-submitted</c>,
/// <c>webhook-pull-request-review-edited</c>, and <c>webhook-pull-request-review-dismissed</c> schemas.
/// </summary>
internal sealed record GitHubPullRequestReviewWebhookPayload
{
    public string Action { get; init; } = "";

    public GitHubWebhookReview Review { get; init; } = new();

    public GitHubWebhookPullRequest PullRequest { get; init; } = new();

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }
}

/// <summary>
/// Models GitHub's official <c>webhook-pull-request-review-comment-created</c> schema.
/// </summary>
internal sealed record GitHubPullRequestReviewCommentWebhookPayload
{
    public string Action { get; init; } = "";

    public GitHubWebhookComment Comment { get; init; } = new();

    public GitHubWebhookPullRequest PullRequest { get; init; } = new();

    public GitHubWebhookRepository Repository { get; init; } = new();

    public GitHubWebhookUser? Sender { get; init; }
}

/// <summary>
/// Models GitHub's official <c>webhook-ping</c> schema.
/// </summary>
internal sealed record GitHubPingWebhookPayload
{
    public GitHubWebhookRepository? Repository { get; init; }
}

internal sealed record GitHubWebhookRepository
{
    public long Id { get; init; }

    public string FullName { get; init; } = "";

    public string HtmlUrl { get; init; } = "";
}

internal sealed record GitHubWebhookUser
{
    public long Id { get; init; }

    public string Login { get; init; } = "";
}

internal sealed record GitHubWebhookIssue
{
    public long Id { get; init; }

    public int Number { get; init; }

    public string Title { get; init; } = "";

    public string HtmlUrl { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public GitHubWebhookUser? User { get; init; }

    public GitHubWebhookPullRequestReference? PullRequest { get; init; }
}

internal sealed record GitHubWebhookPullRequestReference;

internal sealed record GitHubWebhookPullRequest
{
    public long Id { get; init; }

    public int Number { get; init; }

    public string Title { get; init; } = "";

    public string HtmlUrl { get; init; } = "";

    public bool Merged { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public DateTimeOffset? MergedAt { get; init; }

    public GitHubWebhookUser? User { get; init; }
}

internal sealed record GitHubWebhookComment
{
    public long Id { get; init; }

    public string Body { get; init; } = "";

    public string HtmlUrl { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public GitHubWebhookUser? User { get; init; }

    public long? InReplyToId { get; init; }
}

internal sealed record GitHubWebhookReview
{
    public long Id { get; init; }

    public string? Body { get; init; }

    public string State { get; init; } = "";

    public string HtmlUrl { get; init; } = "";

    public DateTimeOffset? SubmittedAt { get; init; }

    public GitHubWebhookUser? User { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubPushWebhookPayload))]
[JsonSerializable(typeof(GitHubIssuesWebhookPayload))]
[JsonSerializable(typeof(GitHubIssueCommentWebhookPayload))]
[JsonSerializable(typeof(GitHubPullRequestWebhookPayload))]
[JsonSerializable(typeof(GitHubPullRequestReviewWebhookPayload))]
[JsonSerializable(typeof(GitHubPullRequestReviewCommentWebhookPayload))]
[JsonSerializable(typeof(GitHubPingWebhookPayload))]
internal sealed partial class GitHubWebhookJsonSerializerContext : JsonSerializerContext;
