using System.Text.RegularExpressions;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.GitHub;

public static partial class GitHubEventMapper
{
    private const int MaximumSummaryLength = 108;

    public static IReadOnlyList<ForgeEvent> Map(IEnumerable<GitHubActivityEvent> activityEvents)
    {
        List<ForgeEvent> mappedEvents = [];

        foreach (var activityEvent in activityEvents)
        {
            MapEvent(activityEvent, mappedEvents);
        }

        return mappedEvents;
    }

    private static void MapEvent(GitHubActivityEvent activityEvent, List<ForgeEvent> destination)
    {
        if (string.IsNullOrWhiteSpace(activityEvent.Type) ||
            string.IsNullOrWhiteSpace(activityEvent.Id) ||
            string.IsNullOrWhiteSpace(activityEvent.Repository.Name))
        {
            return;
        }

        var createdAt = activityEvent.CreatedAt ?? DateTimeOffset.UtcNow;
        switch (activityEvent.Type)
        {
            case "PushEvent":
                MapCommit(activityEvent, createdAt, destination);
                break;
            case "IssuesEvent":
                MapIssue(activityEvent, createdAt, destination);
                break;
            case "IssueCommentEvent":
                MapIssueComment(activityEvent, createdAt, destination);
                break;
            case "PullRequestReviewCommentEvent":
                MapReviewComment(activityEvent, createdAt, destination);
                break;
            case "PullRequestReviewEvent":
                MapReview(activityEvent, createdAt, destination);
                break;
            case "PullRequestEvent":
                MapPullRequest(activityEvent, createdAt, destination);
                break;
        }
    }

    private static void MapCommit(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        string? head = activityEvent.Payload.Head;
        if (string.IsNullOrWhiteSpace(head))
        {
            return;
        }

        string? branch = activityEvent.Payload.Ref?.Replace(
            "refs/heads/",
            "",
            StringComparison.Ordinal);
        string shortSha = head[..Math.Min(7, head.Length)];
        string summary = string.IsNullOrWhiteSpace(branch)
            ? $"pushed commit {shortSha}"
            : $"pushed {shortSha} to {branch}";

        destination.Add(new ForgeEvent(
            $"{activityEvent.Id}:{head}",
            ForgeEventKind.Commit,
            activityEvent.Repository.Name,
            activityEvent.Actor.Login,
            summary,
            $"https://github.com/{activityEvent.Repository.Name}/commit/{head}",
            createdAt));
    }

    private static void MapIssue(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        var payload = activityEvent.Payload;
        if (payload.Issue is not { PullRequest: null } issue || issue.Number <= 0)
        {
            return;
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
            return;
        }

        string repository = activityEvent.Repository.Name;
        string url = Fallback(
            issue.HtmlUrl,
            $"https://github.com/{repository}/issues/{issue.Number}");
        destination.Add(new ForgeEvent(
            activityEvent.Id,
            kind.Value,
            repository,
            activityEvent.Actor.Login,
            BuildLifecycleSummary(payload.Action!, "issue", issue.Number, issue.Title),
            url,
            createdAt));
    }

    private static void MapIssueComment(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        var payload = activityEvent.Payload;
        if (payload.Action != "created" || payload.Issue is not { } issue)
        {
            return;
        }

        if (issue.PullRequest is null)
        {
            AddIssueComment(activityEvent, issue, createdAt, destination);
        }
        else
        {
            AddChangeRequestComment(activityEvent, issue.Number, createdAt, destination, false);
        }
    }

    private static void AddIssueComment(
        GitHubActivityEvent activityEvent,
        GitHubIssue issue,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        var comment = activityEvent.Payload.Comment;
        if (comment is null || issue.Number <= 0)
        {
            return;
        }

        string repository = activityEvent.Repository.Name;
        string body = CleanSummary(comment.Body ?? "");
        string url = Fallback(
            comment.HtmlUrl,
            Fallback(issue.HtmlUrl, $"https://github.com/{repository}/issues/{issue.Number}"));
        string summary = string.IsNullOrWhiteSpace(body)
            ? $"commented on issue #{issue.Number}"
            : body;

        destination.Add(new ForgeEvent(
            $"{activityEvent.Id}:{comment.Id}",
            ForgeEventKind.IssueComment,
            repository,
            Fallback(comment.User?.Login, activityEvent.Actor.Login),
            summary,
            url,
            createdAt));
    }

    private static void MapReviewComment(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        if (activityEvent.Payload.Action == "created")
        {
            AddChangeRequestComment(
                activityEvent,
                activityEvent.Payload.PullRequest?.Number,
                createdAt,
                destination,
                true);
        }
    }

    private static void AddChangeRequestComment(
        GitHubActivityEvent activityEvent,
        int? pullRequestNumber,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination,
        bool canBeReply)
    {
        var comment = activityEvent.Payload.Comment;
        if (comment is null)
        {
            return;
        }

        string repository = activityEvent.Repository.Name;
        string body = CleanSummary(comment.Body ?? "");
        string url = Fallback(
            comment.HtmlUrl,
            pullRequestNumber is null
                ? ""
                : $"https://github.com/{repository}/pull/{pullRequestNumber}");
        string summary = string.IsNullOrWhiteSpace(body)
            ? $"commented on {GitHubProviderMetadata.ChangeRequestSingular} #{pullRequestNumber}"
            : body;

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        destination.Add(new ForgeEvent(
            $"{activityEvent.Id}:{comment.Id}",
            canBeReply && comment.InReplyToId is not null
                ? ForgeEventKind.ChangeRequestReply
                : ForgeEventKind.ChangeRequestComment,
            repository,
            Fallback(comment.User?.Login, activityEvent.Actor.Login),
            summary,
            url,
            createdAt));
    }

    private static void MapReview(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        var payload = activityEvent.Payload;
        if (payload.Action is not ("created" or "updated" or "dismissed") ||
            payload.Review is not { } review)
        {
            return;
        }

        string repository = activityEvent.Repository.Name;
        int? number = payload.PullRequest?.Number;
        string body = CleanSummary(review.Body ?? "");
        string state = review.State.Replace('_', ' ').ToLowerInvariant();
        string url = Fallback(
            review.HtmlUrl,
            number is null ? "" : $"https://github.com/{repository}/pull/{number}");
        string summary = string.IsNullOrWhiteSpace(body)
            ? CleanSummary($"{Fallback(state, "reviewed")} {GitHubProviderMetadata.ChangeRequestSingular} #{number}")
            : body;

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        destination.Add(new ForgeEvent(
            $"{activityEvent.Id}:{review.Id}",
            ForgeEventKind.ChangeRequestReview,
            repository,
            Fallback(review.User?.Login, activityEvent.Actor.Login),
            summary,
            url,
            createdAt));
    }

    private static void MapPullRequest(
        GitHubActivityEvent activityEvent,
        DateTimeOffset createdAt,
        List<ForgeEvent> destination)
    {
        var payload = activityEvent.Payload;
        if (payload.Number is not { } number || number <= 0)
        {
            return;
        }

        ForgeEventKind? kind = payload.Action switch
        {
            "opened" => ForgeEventKind.ChangeRequestOpened,
            "closed" => ForgeEventKind.ChangeRequestClosed,
            "reopened" => ForgeEventKind.ChangeRequestReopened,
            "merged" => ForgeEventKind.ChangeRequestMerge,
            _ => null
        };
        if (kind is null)
        {
            return;
        }

        string repository = activityEvent.Repository.Name;
        string url = Fallback(
            payload.PullRequest?.HtmlUrl,
            $"https://github.com/{repository}/pull/{number}");
        destination.Add(new ForgeEvent(
            activityEvent.Id,
            kind.Value,
            repository,
            activityEvent.Actor.Login,
            BuildLifecycleSummary(
                payload.Action!,
                GitHubProviderMetadata.ChangeRequestSingular,
                number,
                payload.PullRequest?.Title),
            url,
            createdAt));
    }

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

    private static string Fallback(string? preferred, string fallback) =>
        string.IsNullOrWhiteSpace(preferred) ? fallback : preferred;

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
