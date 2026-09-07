using System.Text.Json;
using System.Text.RegularExpressions;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.GitHub;

internal static partial class GitHubWebhookMapper
{
    private const int MaximumSummaryLength = 108;

    public static GitHubWebhookMapping Map(
        string eventName,
        string deliveryId,
        ReadOnlySpan<byte> body,
        DateTimeOffset receivedAt)
    {
        return eventName switch
        {
            "push" => MapPush(DeserializePush(body), deliveryId, receivedAt),
            "issues" => MapIssue(DeserializeIssues(body), deliveryId, receivedAt),
            "issue_comment" => MapIssueComment(DeserializeIssueComment(body), deliveryId, receivedAt),
            "pull_request" => MapPullRequest(DeserializePullRequest(body), deliveryId, receivedAt),
            "pull_request_review" => MapReview(DeserializeReview(body), deliveryId, receivedAt),
            "pull_request_review_comment" => MapReviewComment(
                DeserializeReviewComment(body),
                deliveryId,
                receivedAt),
            "ping" => MapPing(DeserializePing(body)),
            _ => GitHubWebhookMapping.Unsupported
        };
    }

    private static GitHubWebhookMapping MapPush(
        GitHubPushWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData || payload.Commits is null)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        if (string.IsNullOrWhiteSpace(repository))
        {
            return GitHubWebhookMapping.Invalid;
        }

        string reference = payload.Ref ?? "";
        string branch = reference.StartsWith("refs/heads/", StringComparison.Ordinal)
            ? reference["refs/heads/".Length..]
            : reference;
        List<ForgeEvent> events = [];
        foreach (GitHubWebhookCommit commit in payload.Commits)
        {
            if (string.IsNullOrWhiteSpace(commit.Id))
            {
                continue;
            }

            string shortSha = commit.Id[..Math.Min(7, commit.Id.Length)];
            string summary = string.IsNullOrWhiteSpace(branch)
                ? $"pushed commit {shortSha}"
                : $"pushed to {branch}";
            string message = CleanSummary(commit.Message ?? "");
            if (!string.IsNullOrWhiteSpace(message))
            {
                summary = CleanSummary($"{summary}: {message}");
            }

            events.Add(new ForgeEvent(
                $"{deliveryId}:{commit.Id}",
                ForgeEventKind.Committed,
                repository,
                FirstNonEmpty(
                    payload.Sender?.Login,
                    payload.Pusher?.Username,
                    payload.Pusher?.Name),
                summary,
                $"{RepositoryUrl(repositoryData)}/commit/{commit.Id}",
                receivedAt));
        }

        return GitHubWebhookMapping.Valid(repository, events);
    }

    private static GitHubWebhookMapping MapIssue(
        GitHubIssuesWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData || payload.Issue is not { } issue)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        if (string.IsNullOrWhiteSpace(repository) || issue.Number <= 0)
        {
            return GitHubWebhookMapping.Invalid;
        }

        ForgeEventKind? kind = payload.Action switch
        {
            "opened" => ForgeEventKind.IssueOpened,
            "closed" => ForgeEventKind.IssueClosed,
            "reopened" => ForgeEventKind.IssueReopened,
            _ => null
        };
        if (kind is null)
        {
            return GitHubWebhookMapping.Valid(repository, []);
        }

        DateTimeOffset createdAt = payload.Action switch
        {
            "opened" => issue.CreatedAt ?? receivedAt,
            "closed" => issue.ClosedAt ?? receivedAt,
            "reopened" => issue.UpdatedAt ?? receivedAt,
            _ => receivedAt
        };
        ForgeEvent forgeEvent = new(
            deliveryId,
            kind.Value,
            repository,
            FirstNonEmpty(payload.Sender?.Login, issue.User?.Login),
            BuildLifecycleSummary(payload.Action, "issue", issue.Number, issue.Title),
            FirstNonEmpty(issue.HtmlUrl, $"{RepositoryUrl(repositoryData)}/issues/{issue.Number}"),
            createdAt);
        return GitHubWebhookMapping.Valid(repository, [forgeEvent]);
    }

    private static GitHubWebhookMapping MapIssueComment(
        GitHubIssueCommentWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData ||
            payload.Issue is not { } issue ||
            payload.Comment is not { } comment)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        if (string.IsNullOrWhiteSpace(repository) ||
            issue.Number <= 0 ||
            comment.Id <= 0)
        {
            return GitHubWebhookMapping.Invalid;
        }

        if (payload.Action != "created")
        {
            return GitHubWebhookMapping.Valid(repository, []);
        }

        bool isChangeRequest = issue.PullRequest is not null;
        string body = CleanSummary(comment.Body ?? "");
        string summary = string.IsNullOrWhiteSpace(body)
            ? isChangeRequest
                ? $"commented on {GitHubProviderMetadata.ChangeRequestSingular} #{issue.Number}"
                : $"commented on issue #{issue.Number}"
            : body;
        ForgeEvent forgeEvent = new(
            $"{deliveryId}:{comment.Id}",
            isChangeRequest
                ? ForgeEventKind.ChangeRequestCommented
                : ForgeEventKind.IssueCommented,
            repository,
            FirstNonEmpty(comment.User?.Login, payload.Sender?.Login),
            summary,
            FirstNonEmpty(
                comment.HtmlUrl,
                issue.HtmlUrl,
                $"{RepositoryUrl(repositoryData)}/issues/{issue.Number}"),
            comment.CreatedAt ?? receivedAt);
        return GitHubWebhookMapping.Valid(repository, [forgeEvent]);
    }

    private static GitHubWebhookMapping MapPullRequest(
        GitHubPullRequestWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData ||
            payload.PullRequest is not { } pullRequest)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        int number = payload.Number > 0 ? payload.Number : pullRequest.Number;
        if (string.IsNullOrWhiteSpace(repository) || number <= 0)
        {
            return GitHubWebhookMapping.Invalid;
        }

        ForgeEventKind? kind = payload.Action switch
        {
            "opened" => ForgeEventKind.ChangeRequestOpened,
            "reopened" => ForgeEventKind.ChangeRequestReopened,
            "closed" when pullRequest.Merged => ForgeEventKind.ChangeRequestMerged,
            "closed" => ForgeEventKind.ChangeRequestClosed,
            _ => null
        };
        if (kind is null)
        {
            return GitHubWebhookMapping.Valid(repository, []);
        }

        string action = kind == ForgeEventKind.ChangeRequestMerged ? "merged" : payload.Action;
        DateTimeOffset createdAt = kind switch
        {
            ForgeEventKind.ChangeRequestOpened => pullRequest.CreatedAt ?? receivedAt,
            ForgeEventKind.ChangeRequestMerged => pullRequest.MergedAt ?? receivedAt,
            ForgeEventKind.ChangeRequestClosed => pullRequest.ClosedAt ?? receivedAt,
            ForgeEventKind.ChangeRequestReopened => pullRequest.UpdatedAt ?? receivedAt,
            _ => receivedAt
        };
        ForgeEvent forgeEvent = new(
            deliveryId,
            kind.Value,
            repository,
            FirstNonEmpty(payload.Sender?.Login, pullRequest.User?.Login),
            BuildLifecycleSummary(
                action,
                GitHubProviderMetadata.ChangeRequestSingular,
                number,
                pullRequest.Title),
            FirstNonEmpty(
                pullRequest.HtmlUrl,
                $"{RepositoryUrl(repositoryData)}/pull/{number}"),
            createdAt);
        return GitHubWebhookMapping.Valid(repository, [forgeEvent]);
    }

    private static GitHubWebhookMapping MapReview(
        GitHubPullRequestReviewWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData ||
            payload.PullRequest is not { } pullRequest ||
            payload.Review is not { } review)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        if (string.IsNullOrWhiteSpace(repository) ||
            pullRequest.Number <= 0 ||
            review.Id <= 0)
        {
            return GitHubWebhookMapping.Invalid;
        }

        if (payload.Action is not ("submitted" or "edited" or "dismissed"))
        {
            return GitHubWebhookMapping.Valid(repository, []);
        }

        string body = CleanSummary(review.Body ?? "");
        string state = CleanSummary((review.State ?? "").Replace('_', ' ').ToLowerInvariant());
        string summary = payload.Action switch
        {
            "submitted" when !string.IsNullOrWhiteSpace(body) => body,
            "submitted" => $"{FirstNonEmpty(state, "reviewed")} {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequest.Number}",
            "edited" when !string.IsNullOrWhiteSpace(body) =>
                CleanSummary($"edited review on {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequest.Number}: {body}"),
            "edited" => $"edited review on {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequest.Number}",
            _ => $"dismissed review on {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequest.Number}"
        };
        bool isSubmitted = payload.Action == "submitted";
        ForgeEvent forgeEvent = new(
            $"{deliveryId}:{review.Id}",
            ForgeEventKind.ChangeRequestReviewed,
            repository,
            isSubmitted
                ? FirstNonEmpty(review.User?.Login, payload.Sender?.Login)
                : FirstNonEmpty(payload.Sender?.Login, review.User?.Login),
            summary,
            FirstNonEmpty(review.HtmlUrl, pullRequest.HtmlUrl),
            isSubmitted ? review.SubmittedAt ?? receivedAt : receivedAt);
        return GitHubWebhookMapping.Valid(repository, [forgeEvent]);
    }

    private static GitHubWebhookMapping MapReviewComment(
        GitHubPullRequestReviewCommentWebhookPayload payload,
        string deliveryId,
        DateTimeOffset receivedAt)
    {
        if (payload.Repository is not { } repositoryData ||
            payload.PullRequest is not { } pullRequest ||
            payload.Comment is not { } comment)
        {
            return GitHubWebhookMapping.Invalid;
        }

        string repository = repositoryData.FullName;
        if (string.IsNullOrWhiteSpace(repository) ||
            pullRequest.Number <= 0 ||
            comment.Id <= 0)
        {
            return GitHubWebhookMapping.Invalid;
        }

        if (payload.Action != "created")
        {
            return GitHubWebhookMapping.Valid(repository, []);
        }

        string body = CleanSummary(comment.Body ?? "");
        string summary = string.IsNullOrWhiteSpace(body)
            ? $"commented on {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequest.Number}"
            : body;
        ForgeEvent forgeEvent = new(
            $"{deliveryId}:{comment.Id}",
            comment.InReplyToId is null
                ? ForgeEventKind.ChangeRequestCommented
                : ForgeEventKind.ChangeRequestReplied,
            repository,
            FirstNonEmpty(comment.User?.Login, payload.Sender?.Login),
            summary,
            FirstNonEmpty(comment.HtmlUrl, pullRequest.HtmlUrl),
            comment.CreatedAt ?? receivedAt);
        return GitHubWebhookMapping.Valid(repository, [forgeEvent]);
    }

    private static GitHubWebhookMapping MapPing(GitHubPingWebhookPayload payload)
    {
        string? repository = payload.Repository?.FullName;
        return payload.Repository is null || !string.IsNullOrWhiteSpace(repository)
            ? GitHubWebhookMapping.Valid(repository, [])
            : GitHubWebhookMapping.Invalid;
    }

    private static GitHubPushWebhookPayload DeserializePush(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubPushWebhookPayload)!;

    private static GitHubIssuesWebhookPayload DeserializeIssues(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubIssuesWebhookPayload)!;

    private static GitHubIssueCommentWebhookPayload DeserializeIssueComment(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubIssueCommentWebhookPayload)!;

    private static GitHubPullRequestWebhookPayload DeserializePullRequest(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubPullRequestWebhookPayload)!;

    private static GitHubPullRequestReviewWebhookPayload DeserializeReview(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubPullRequestReviewWebhookPayload)!;

    private static GitHubPullRequestReviewCommentWebhookPayload DeserializeReviewComment(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(
            body,
            GitHubWebhookJsonSerializerContext.Default.GitHubPullRequestReviewCommentWebhookPayload)!;

    private static GitHubPingWebhookPayload DeserializePing(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, GitHubWebhookJsonSerializerContext.Default.GitHubPingWebhookPayload)!;

    private static string RepositoryUrl(GitHubWebhookRepository repository) =>
        FirstNonEmpty(repository.HtmlUrl, $"https://github.com/{repository.FullName}");

    private static string BuildLifecycleSummary(
        string action,
        string resource,
        int number,
        string? title)
    {
        string prefix = $"{action} {resource} #{number}";
        return string.IsNullOrWhiteSpace(title)
            ? prefix
            : CleanSummary($"{prefix}: {title}");
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "unknown";

    private static string CleanSummary(string value)
    {
        string withoutHtmlComments = HtmlComments()
            .Replace(GitHubEmojiShortcodes.Emojify(value), "");
        string withoutHtmlTags = HtmlTags().Replace(withoutHtmlComments, " ");
        string summary = Whitespace()
            .Replace(withoutHtmlTags, " ")
            .Trim();
        if (summary.Length <= MaximumSummaryLength)
        {
            return summary;
        }

        return $"{summary[..(MaximumSummaryLength - 1)].TrimEnd()}…";
    }

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlComments();

    [GeneratedRegex(@"</?[A-Za-z][^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTags();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}

internal sealed record GitHubWebhookMapping(
    bool IsSupported,
    bool IsValid,
    string? Repository,
    IReadOnlyList<ForgeEvent> Events)
{
    public static GitHubWebhookMapping Unsupported { get; } = new(false, true, null, []);

    public static GitHubWebhookMapping Invalid { get; } = new(true, false, null, []);

    public static GitHubWebhookMapping Valid(
        string? repository,
        IReadOnlyList<ForgeEvent> events) => new(true, true, repository, events);
}
