using System.Text;
using AutoMapper.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.IdentityModel.Tokens;

namespace AutoMapper.UnitTests.Licensing;

public class LicenseAccessorTests
{
    // Structurally valid compact JWS (correct issuer, audience and kid, 342-character RS256 signature)
    // whose signature is not genuine, so it reaches signature validation and fails there.
    private static readonly string WellFormedKey = string.Join(".",
        Base64Url("""{"alg":"RS256","kid":"LuckyPennySoftwareLicenseKey/bbb13acb59904d89b4cb1c85f088ccf9","typ":"JWT"}"""),
        Base64Url("""{"iss":"https://luckypennysoftware.com","aud":"LuckyPennySoftware"}"""),
        new string('A', 342));

    [Fact]
    public void WellFormedKey_PassesStructuralCheck() =>
        LicenseAccessor.IsWellFormed(WellFormedKey).ShouldBeTrue();

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public void TruncatedKey_LogsIncompleteMessage(int missingCharacters)
    {
        var truncatedKey = WellFormedKey[..^missingCharacters];

        var logs = Validate(truncatedKey);

        ShouldLogIncompleteKey(logs, truncatedKey);
    }

    [Theory]
    [InlineData("header.payload")]
    [InlineData("header.payload.signature.extra")]
    [InlineData("not-a-real-license-key")]
    public void WrongSegmentCount_LogsIncompleteMessage(string key)
    {
        var logs = Validate(key);

        ShouldLogIncompleteKey(logs, key);
    }

    [Fact]
    public void NonBase64UrlCharacters_LogIncompleteMessage()
    {
        var key = WellFormedKey[..^1] + "=";

        var logs = Validate(key);

        ShouldLogIncompleteKey(logs, key);
    }

    [Fact]
    public void InvalidSignature_LogsCriticalWithCopyPasteHint()
    {
        var logs = Validate(WellFormedKey);

        var log = logs.ShouldHaveSingleItem();
        log.Level.ShouldBe(LogLevel.Critical);
        log.Exception.ShouldBeOfType<SecurityTokenInvalidSignatureException>();
        log.Message.ShouldContain("copy the full key again", Case.Sensitive);
    }

    [Fact]
    public void NestedPlatformNotSupportedException_LogsClientRedistributionInfo()
    {
        var exception = new SecurityTokenInvalidSignatureException("IDX10511",
            new InvalidOperationException("wrapper", new PlatformNotSupportedException()));

        var logs = LogValidationFailure(exception);

        ShouldLogClientRedistribution(logs);
    }

    [Fact]
    public void AggregatedPlatformNotSupportedException_LogsClientRedistributionInfo()
    {
        var exception = new SecurityTokenInvalidSignatureException("IDX10511",
            new AggregateException(new InvalidOperationException(), new PlatformNotSupportedException()));

        var logs = LogValidationFailure(exception);

        ShouldLogClientRedistribution(logs);
    }

    [Fact]
    public void OtherValidationFailure_LogsCritical()
    {
        var logs = LogValidationFailure(new SecurityTokenInvalidIssuerException("IDX10205"));

        logs.ShouldHaveSingleItem().Level.ShouldBe(LogLevel.Critical);
    }

    private static IReadOnlyList<FakeLogRecord> Validate(string key) =>
        WithAccessor(accessor => accessor.ValidateKey(key).ShouldBeEmpty());

    private static IReadOnlyList<FakeLogRecord> LogValidationFailure(Exception exception) =>
        WithAccessor(accessor => accessor.LogValidationFailure(exception));

    private static IReadOnlyList<FakeLogRecord> WithAccessor(Action<LicenseAccessor> act)
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        act(new LicenseAccessor(null, factory));

        return provider.Collector.GetSnapshot();
    }

    private static void ShouldLogIncompleteKey(IReadOnlyList<FakeLogRecord> logs, string key)
    {
        var log = logs.ShouldHaveSingleItem();
        log.Level.ShouldBe(LogLevel.Error);
        log.Exception.ShouldBeNull();
        log.Message.ShouldContain("incomplete or corrupted", Case.Sensitive);
        log.Message.ShouldNotContain(key);
    }

    private static void ShouldLogClientRedistribution(IReadOnlyList<FakeLogRecord> logs)
    {
        var log = logs.ShouldHaveSingleItem();
        log.Level.ShouldBe(LogLevel.Information);
        log.Message.ShouldContain("client-redistribution-scenarios", Case.Sensitive);
    }

    private static string Base64Url(string json) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
