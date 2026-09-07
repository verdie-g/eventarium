using System.Globalization;
using System.Runtime.CompilerServices;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.Fake;

public sealed class FakeForgeEventProvider : IForgeEventProvider
{
    private const int InitialEventCount = 4;
    private const string RuntimeRepository = "dotnet/runtime";
    private static readonly TimeSpan DefaultEventInterval = TimeSpan.FromSeconds(3);
    private static readonly ForgeProviderStatus StreamingStatus = new(ForgeConnectionState.Streaming);
    private static readonly RecordedEventTemplate[] RuntimeEventTemplates =
    [
        new(
            ForgeEventKind.ChangeRequestReview,
            "EgorBo",
            "commented pull request #133304",
            "https://github.com/dotnet/runtime/pull/133304#pullrequestreview-5121884037",
            ParseTimestamp("2026-09-05T15:45:25Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "I have opened https://github.com/dotnet/runtime/issues/133305 to track adding this test coverage back",
            "https://github.com/dotnet/runtime/pull/133294#issuecomment-5552910024",
            ParseTimestamp("2026-09-05T15:42:33Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "steveisok",
            "Closing - will fix on main and backport again.",
            "https://github.com/dotnet/runtime/pull/133179#issuecomment-5552904900",
            ParseTimestamp("2026-09-05T15:41:44Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestMerge,
            "Copilot",
            "merged pull request #133294",
            "https://github.com/dotnet/runtime/pull/133294",
            ParseTimestamp("2026-09-05T15:40:05Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "/ba-g existing failures",
            "https://github.com/dotnet/runtime/pull/133294#issuecomment-5552888534",
            ParseTimestamp("2026-09-05T15:38:59Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "steveisok",
            "approved pull request #133295",
            "https://github.com/dotnet/runtime/pull/133295#pullrequestreview-5121861307",
            ParseTimestamp("2026-09-05T15:37:46Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "svick",
            "pushed e5163aa to main",
            "https://github.com/dotnet/runtime/commit/e5163aae31271323e11f475cb21924bf9d760adc",
            ParseTimestamp("2026-09-03T12:22:19Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "> you need to attach dotnet-trace before your app started JIT, or you're missing some of the mappings You s…",
            "https://github.com/dotnet/runtime/pull/132948#issuecomment-5552693396",
            ParseTimestamp("2026-09-05T15:06:17Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "dotnet-maestro[bot]",
            "> [!IMPORTANT] > While this PR was open, the source repository has received code changes from this reposito…",
            "https://github.com/dotnet/runtime/pull/133198#issuecomment-5552680399",
            ParseTimestamp("2026-09-05T15:03:57Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "jkotas",
            "commented pull request #131826",
            "https://github.com/dotnet/runtime/pull/131826#pullrequestreview-5121698011",
            ParseTimestamp("2026-09-05T14:54:34Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "> If we could consistently use the jmpabs encoding everywhere when the hardware supported it then we could…",
            "https://github.com/dotnet/runtime/pull/131826#discussion_r3940985807",
            ParseTimestamp("2026-09-05T14:54:33Z"),
            true),
        new(
            ForgeEventKind.Commit,
            "rosebyte",
            "pushed 6fa6b31 to main",
            "https://github.com/dotnet/runtime/commit/6fa6b312059df1391964394cde231c78b8c6b0db",
            ParseTimestamp("2026-09-04T18:27:02Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "jkotas",
            "commented pull request #133304",
            "https://github.com/dotnet/runtime/pull/133304#pullrequestreview-5121633830",
            ParseTimestamp("2026-09-05T14:44:18Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "This should be moved to AMD64-only note in \"Return buffers\" section",
            "https://github.com/dotnet/runtime/pull/133304#discussion_r3940953011",
            ParseTimestamp("2026-09-05T14:44:16Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "jkotas",
            "commented pull request #133304",
            "https://github.com/dotnet/runtime/pull/133304#pullrequestreview-5121627596",
            ParseTimestamp("2026-09-05T14:42:57Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jkotas",
            "This applies to all architectures now. This section should be reworded so that it does not start with expla…",
            "https://github.com/dotnet/runtime/pull/133304#discussion_r3940949996",
            ParseTimestamp("2026-09-05T14:42:55Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "tommcdon",
            "pushed 0b9ae3c to main",
            "https://github.com/dotnet/runtime/commit/0b9ae3c0c9087d43d107a3e5e7149ce03ef8bc28",
            ParseTimestamp("2026-09-05T14:21:07Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "Copilot",
            "This behavioral codegen change has no regression coverage in the PR. Please add a targeted JIT test for a r…",
            "https://github.com/dotnet/runtime/pull/133304#discussion_r3940927903",
            ParseTimestamp("2026-09-05T14:33:22Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "AndyAyersMS",
            "approved pull request #133294",
            "https://github.com/dotnet/runtime/pull/133294#pullrequestreview-5121571654",
            ParseTimestamp("2026-09-05T14:25:32Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "akoeplinger",
            "pushed aa6daff to release/10.0",
            "https://github.com/dotnet/runtime/commit/aa6daffd5bf5e38f514af67a57a6e8b8b741d5f8",
            ParseTimestamp("2026-09-03T17:33:49Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "dotnet-policy-service[bot]",
            "Tagging subscribers to this area: @JulieLeeMSFT, @jakobbotsch See info in [area-owners.md](https://github.c…",
            "https://github.com/dotnet/runtime/pull/133304#issuecomment-5552456168",
            ParseTimestamp("2026-09-05T14:25:12Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 6 pipeline(s).<br> 10 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133304#issuecomment-5552449759",
            ParseTimestamp("2026-09-05T14:24:04Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 3 pipeline(s).<br> 13 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133295#issuecomment-5552444682",
            ParseTimestamp("2026-09-05T14:23:09Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestMerge,
            "tommcdon",
            "merged pull request #133253",
            "https://github.com/dotnet/runtime/pull/133253",
            ParseTimestamp("2026-09-05T14:21:06Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "lewing",
            "pushed a5b8d11 to main",
            "https://github.com/dotnet/runtime/commit/a5b8d110306eff632cced48a4d108d619393a6a4",
            ParseTimestamp("2026-09-04T16:31:34Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "steveisok",
            "pushed a17fc71 to release/11.0",
            "https://github.com/dotnet/runtime/commit/a17fc71de96acde68502faa13fa031b132609de3",
            ParseTimestamp("2026-09-04T14:48:57Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "lewing",
            "pushed 1b46b18 to fix-133120-test-execution",
            "https://github.com/dotnet/runtime/commit/1b46b189f77657b22eb1b977e72e1854be050c09",
            ParseTimestamp("2026-09-03T16:55:58Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "svick",
            "pushed 10f4939 to release/11.0",
            "https://github.com/dotnet/runtime/commit/10f493968919024f835b5c485b5961079ddf95f9",
            ParseTimestamp("2026-09-03T16:42:39Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "svick",
            "pushed f7d27a6 to release/10.0",
            "https://github.com/dotnet/runtime/commit/f7d27a6d8a96303c1105ad8a30931378f83f3ab0",
            ParseTimestamp("2026-09-03T16:58:45Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "rzikm",
            "pushed 77c8032 to release/9.0-staging",
            "https://github.com/dotnet/runtime/commit/77c80323be220c3f8eb3845d7fc4d5e2a5ae6f31",
            ParseTimestamp("2026-09-03T16:40:54Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "EgorBo",
            "approved pull request #133079",
            "https://github.com/dotnet/runtime/pull/133079#pullrequestreview-5121172072",
            ParseTimestamp("2026-09-05T11:55:44Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "jkotas",
            "pushed 9b3c096 to main",
            "https://github.com/dotnet/runtime/commit/9b3c09618a80f73d44c8b9747a33cc6523c3b3fc",
            ParseTimestamp("2026-09-05T07:11:17Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "AndyAyersMS",
            "pushed 0f4ec71 to main",
            "https://github.com/dotnet/runtime/commit/0f4ec71b02b96e4c9823a2c9bd6a52dc0fbdb9fa",
            ParseTimestamp("2026-09-03T15:54:31Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 3 pipeline(s).<br> 13 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133300#issuecomment-5551353818",
            ParseTimestamp("2026-09-05T11:07:49Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestMerge,
            "EgorBo",
            "merged pull request #132920",
            "https://github.com/dotnet/runtime/pull/132920",
            ParseTimestamp("2026-09-05T11:02:18Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "AndyAyersMS",
            "pushed 334cc5c to main",
            "https://github.com/dotnet/runtime/commit/334cc5c43dfb4b6acab5052c15ee8b1631d069bc",
            ParseTimestamp("2026-09-03T14:26:16Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "dotnet-policy-service[bot]",
            "Tagging subscribers to this area: @agocke See info in [area-owners.md](https://github.com/dotnet/runtime/bl…",
            "https://github.com/dotnet/runtime/pull/133300#issuecomment-5551145225",
            ParseTimestamp("2026-09-05T10:22:32Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 3 pipeline(s).<br> 13 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133300#issuecomment-5551143739",
            ParseTimestamp("2026-09-05T10:22:13Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "Copilot",
            "pushed cf65a14 to copilot/fix-checked-narrowing-cast-test-failure",
            "https://github.com/dotnet/runtime/commit/cf65a149256b39dc74d1f04be31d63fd3b1d4db3",
            ParseTimestamp("2026-09-05T07:28:29Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "Copilot",
            "pushed 463630e to copilot/add-disable-runtime-marshalling-again",
            "https://github.com/dotnet/runtime/commit/463630ea82027f84519076130540178ae361dfc2",
            ParseTimestamp("2026-09-05T03:15:26Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "jkoritzinsky",
            "pushed 4b8b9ba to main",
            "https://github.com/dotnet/runtime/commit/4b8b9ba8e4c7b0b1c8f29951a4fb1d3543a7e35b",
            ParseTimestamp("2026-09-05T05:24:56Z"),
            false),
        new(
            ForgeEventKind.Commit,
            "jakobbotsch",
            "pushed e1d442a to main",
            "https://github.com/dotnet/runtime/commit/e1d442a67c5f2202bd2c98e3efe0e2cdc9474d58",
            ParseTimestamp("2026-09-04T11:27:29Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 1 pipeline(s).<br> 15 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133298#issuecomment-5550889748",
            ParseTimestamp("2026-09-05T09:29:31Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "dotnet-policy-service[bot]",
            "Tagging subscribers to this area: @dotnet/runtime-infrastructure See info in [area-owners.md](https://githu…",
            "https://github.com/dotnet/runtime/pull/133298#issuecomment-5550888561",
            ParseTimestamp("2026-09-05T09:29:20Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "azure-pipelines[bot]",
            "<samp> Azure Pipelines:<br> Successfully started running 1 pipeline(s).<br> 15 pipeline(s) were filtered ou…",
            "https://github.com/dotnet/runtime/pull/133298#issuecomment-5550881044",
            ParseTimestamp("2026-09-05T09:27:50Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestMerge,
            "jakobbotsch",
            "merged pull request #133227",
            "https://github.com/dotnet/runtime/pull/133227",
            ParseTimestamp("2026-09-05T09:27:39Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jakobbotsch",
            "/ba-g Failure is #133282 but not matching because of https://github.com/dotnet/arcade/issues/17340",
            "https://github.com/dotnet/runtime/pull/133227#issuecomment-5550879058",
            ParseTimestamp("2026-09-05T09:27:25Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestReview,
            "jakobbotsch",
            "commented pull request #132423",
            "https://github.com/dotnet/runtime/pull/132423#pullrequestreview-5120668196",
            ParseTimestamp("2026-09-05T09:24:16Z"),
            false),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jakobbotsch",
            "Yeah, let me add that.",
            "https://github.com/dotnet/runtime/pull/132423#discussion_r3940132267",
            ParseTimestamp("2026-09-05T09:24:15Z"),
            true),
        new(
            ForgeEventKind.ChangeRequestComment,
            "jakobbotsch",
            "> @jakobbotsch, do we need to backport this to .NET 11? No, this QMARK was introduced by #132539, so .NET 1…",
            "https://github.com/dotnet/runtime/pull/133079#issuecomment-5550689811",
            ParseTimestamp("2026-09-05T08:48:50Z"),
            false)
    ];

    private readonly TimeSpan _eventInterval;
    private readonly IReadOnlyList<RecordedEventTemplate> _eventTemplates;
    private long _sequence;

    public FakeForgeEventProvider(string dataset, TimeSpan? eventInterval = null)
    {
        RecordedForgeDataset recordedDataset = ResolveDataset(dataset);
        Repository = recordedDataset.Repository;
        _eventTemplates = recordedDataset.Events;
        _eventInterval = eventInterval ?? DefaultEventInterval;
    }

    public string Repository { get; }

    public ForgeProviderDescriptor Descriptor { get; } = new(
        "GitHub",
        new ForgeTerminology("pull request", "pull requests"));

    public async IAsyncEnumerable<ForgeEventUpdate> GetUpdatesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ForgeEvent[] initialEvents = Enumerable.Range(0, InitialEventCount)
            .Select(_ => CreateEvent())
            .ToArray();
        yield return new ForgeEventUpdate(initialEvents, StreamingStatus);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_eventInterval, cancellationToken);
            yield return new ForgeEventUpdate([CreateEvent()], StreamingStatus);
        }
    }

    private ForgeEvent CreateEvent()
    {
        long sequence = _sequence++;
        RecordedEventTemplate template = _eventTemplates[(int)(sequence % _eventTemplates.Count)];
        string eventId = sequence.ToString(CultureInfo.InvariantCulture);

        return new ForgeEvent(
            $"fake:{eventId}",
            template.IsReply ? ForgeEventKind.ChangeRequestReply : template.Kind,
            Repository,
            template.Author,
            template.Summary,
            template.Url,
            template.CreatedAt);
    }

    private static RecordedForgeDataset ResolveDataset(string dataset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataset);
        string normalizedDataset = dataset.Trim().ToLowerInvariant();
        return normalizedDataset == "dotnet-runtime"
            ? new RecordedForgeDataset(RuntimeRepository, RuntimeEventTemplates)
            : AdditionalRecordedForgeDatasets.Get(normalizedDataset);
    }

    private static DateTimeOffset ParseTimestamp(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
}
