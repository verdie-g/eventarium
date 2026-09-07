using System.Globalization;
using Eventarium.Core.Forge;

namespace Eventarium.Client.Presentation;

public sealed record DisplayedForgeEvent(
    string DisplayId,
    string EventId,
    ForgeEventKind Kind,
    string SourceName,
    string SourceKind,
    string Repository,
    string Author,
    string Summary,
    string Url,
    string KindCssClass,
    string EventLabel,
    string CssVariables)
{
    private const int MinimumLanePercent = 8;
    private const int MaximumLanePercent = 81;
    private const int MaximumVerticalDriftPixels = 42;
    private const int MinimumArcHeightPixels = 36;
    private const int MaximumArcHeightPixels = 72;
    private const double MinimumBobDurationSeconds = 2.8;
    private const double MaximumBobDurationSeconds = 4.6;
    private const double MinimumRockDurationSeconds = 4.2;
    private const double MaximumRockDurationSeconds = 6.8;
    private const int FirstEventZIndex = 2;

    // Profiles progress from distant, slow objects to nearby, fast objects.
    private static readonly DepthProfile[] DepthProfiles =
    [
        new(0.62, 0.74, 42, 52, 0.82),
        new(0.79, 0.95, 34, 42, 0.90),
        new(0.98, 1.14, 28, 34, 0.98)
    ];

    public static DisplayedForgeEvent Create(
        ForgeFeedEvent feedEvent,
        ForgeSourceDescriptor? source)
    {
        ForgeEvent forgeEvent = feedEvent.Event;
        int depth = Random.Shared.Next(DepthProfiles.Length);
        DepthProfile profile = DepthProfiles[depth];
        double scale = RandomBetween(profile.MinimumScale, profile.MaximumScale);
        double duration = RandomBetween(profile.MinimumDurationSeconds, profile.MaximumDurationSeconds);
        int lane = Random.Shared.Next(MinimumLanePercent, MaximumLanePercent + 1);

        // The signed arc height bends the path above or below its lane. Midpoint combines that arc
        // with the event's end-to-end drift.
        int drift = Random.Shared.Next(-MaximumVerticalDriftPixels, MaximumVerticalDriftPixels + 1);
        int arcHeight = Random.Shared.Next(MinimumArcHeightPixels, MaximumArcHeightPixels + 1) *
            (Random.Shared.Next(2) == 0 ? -1 : 1);
        int midpoint = (int)Math.Round((drift * 0.5) + arcHeight);

        // Independent randomized periods and a negative delay keep nearby events from bobbing
        // and rocking in sync, even when they enter the flight zone at the same time.
        double bobDuration = RandomBetween(MinimumBobDurationSeconds, MaximumBobDurationSeconds);
        double rockDuration = RandomBetween(MinimumRockDurationSeconds, MaximumRockDurationSeconds);
        double motionDelay = -RandomBetween(0, MaximumRockDurationSeconds);

        string cssVariables = FormattableString.Invariant(
            $"--lane:{lane}%;--duration:{duration:0.0}s;--scale:{scale:0.00};--drift:{drift}px;--flight-y-mid:{midpoint}px;--bob-duration:{bobDuration:0.0}s;--rock-duration:{rockDuration:0.0}s;--motion-delay:{motionDelay:0.0}s;--event-opacity:{profile.Opacity:0.00};--layer:{depth + FirstEventZIndex}");

        return new DisplayedForgeEvent(
            Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            feedEvent.Id,
            forgeEvent.Kind,
            source?.DisplayName ?? feedEvent.SourceId,
            source?.Kind ?? "Forge",
            forgeEvent.Repository,
            forgeEvent.Author,
            forgeEvent.Summary,
            forgeEvent.Url,
            GetKindCssClass(forgeEvent.Kind),
            GetEventLabel(forgeEvent.Kind),
            cssVariables);
    }

    private static string GetKindCssClass(ForgeEventKind kind) => kind switch
    {
        ForgeEventKind.Commit => "event-commit",
        ForgeEventKind.IssueOpened or ForgeEventKind.IssueClosed or ForgeEventKind.IssueReopened => "event-issue",
        ForgeEventKind.IssueComment or ForgeEventKind.ChangeRequestComment or ForgeEventKind.ChangeRequestReply => "event-comment",
        ForgeEventKind.ChangeRequestOpened or ForgeEventKind.ChangeRequestClosed or ForgeEventKind.ChangeRequestReopened => "event-change-request",
        ForgeEventKind.ChangeRequestReview => "event-review",
        ForgeEventKind.ChangeRequestMerge => "event-merge",
        _ => string.Empty
    };

    private static string GetEventLabel(ForgeEventKind kind) => kind switch
    {
        ForgeEventKind.Commit => "commit",
        ForgeEventKind.IssueOpened => "issue opened",
        ForgeEventKind.IssueClosed => "issue closed",
        ForgeEventKind.IssueReopened => "issue reopened",
        ForgeEventKind.IssueComment => "issue comment",
        ForgeEventKind.ChangeRequestOpened => "pull request opened",
        ForgeEventKind.ChangeRequestClosed => "pull request closed",
        ForgeEventKind.ChangeRequestReopened => "pull request reopened",
        ForgeEventKind.ChangeRequestComment => "comment",
        ForgeEventKind.ChangeRequestReply => "reply",
        ForgeEventKind.ChangeRequestReview => "review",
        ForgeEventKind.ChangeRequestMerge => "merged",
        _ => "event"
    };

    private static double RandomBetween(double minimum, double maximum) =>
        minimum + (Random.Shared.NextDouble() * (maximum - minimum));

    private sealed record DepthProfile(
        double MinimumScale,
        double MaximumScale,
        double MinimumDurationSeconds,
        double MaximumDurationSeconds,
        double Opacity);
}
