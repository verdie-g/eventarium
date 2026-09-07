namespace Eventarium.Core.Forge;

/// <summary>
/// Identifies the forge activity represented by an event.
/// </summary>
public enum ForgeEventKind
{
    /// <summary>
    /// A commit was created or published.
    /// </summary>
    Commit,

    /// <summary>
    /// An issue was opened.
    /// </summary>
    IssueOpened,

    /// <summary>
    /// An issue was closed.
    /// </summary>
    IssueClosed,

    /// <summary>
    /// An issue was reopened.
    /// </summary>
    IssueReopened,

    /// <summary>
    /// A comment was added to an issue.
    /// </summary>
    IssueComment,

    /// <summary>
    /// A change request was opened.
    /// </summary>
    ChangeRequestOpened,

    /// <summary>
    /// A change request was closed without being merged.
    /// </summary>
    ChangeRequestClosed,

    /// <summary>
    /// A change request was reopened.
    /// </summary>
    ChangeRequestReopened,

    /// <summary>
    /// A comment was added to a change request.
    /// </summary>
    ChangeRequestComment,

    /// <summary>
    /// A reply was added to an existing change-request discussion.
    /// </summary>
    ChangeRequestReply,

    /// <summary>
    /// A review was submitted for a change request.
    /// </summary>
    ChangeRequestReview,

    /// <summary>
    /// A change request was merged.
    /// </summary>
    ChangeRequestMerge
}

/// <summary>
/// Represents one provider-independent repository activity event.
/// </summary>
/// <param name="Id">The provider-defined identifier used to distinguish the event.</param>
/// <param name="Kind">The kind of repository activity.</param>
/// <param name="Repository">The repository identifier suitable for display.</param>
/// <param name="Author">The display name or login of the actor who caused the activity.</param>
/// <param name="Summary">A short, user-facing description of the activity.</param>
/// <param name="Url">The URL of the corresponding resource on the forge.</param>
/// <param name="CreatedAt">The date and time at which the activity occurred.</param>
public sealed record ForgeEvent(
    string Id,
    ForgeEventKind Kind,
    string Repository,
    string Author,
    string Summary,
    string Url,
    DateTimeOffset CreatedAt);
