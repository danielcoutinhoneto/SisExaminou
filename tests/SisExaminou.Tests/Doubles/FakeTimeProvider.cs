namespace SisExaminou.Tests.Doubles;

internal sealed class FakeTimeProvider(DateTimeOffset agoraUtc) : TimeProvider
{
    public DateTimeOffset AgoraUtc { get; set; } = agoraUtc;

    public override DateTimeOffset GetUtcNow() => AgoraUtc;
}
