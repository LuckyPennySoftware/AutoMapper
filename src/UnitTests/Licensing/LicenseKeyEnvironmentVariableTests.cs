using AutoMapper.Licensing;

namespace AutoMapper.UnitTests.Licensing;

// Mutates process-global environment variables, so it must not run alongside
// other tests that read them. Disable parallelization for this class.
[Collection(nameof(LicenseKeyEnvironmentVariableTests))]
[CollectionDefinition(nameof(LicenseKeyEnvironmentVariableTests), DisableParallelization = true)]
public class LicenseKeyEnvironmentVariableTests
{
    private const string AutoMapperEnvVar = LicenseAccessor.AutoMapperLicenseKeyEnvVariable;
    private const string SharedEnvVar = LicenseAccessor.SharedLicenseKeyEnvVariable;
    private const string CleanKey = "header.pay-load.sign_ature";

    public static TheoryData<string> KeysWithWhitespace => new()
    {
        CleanKey + "\n",
        CleanKey + "\r\n",
        CleanKey + " ",
        CleanKey + "\t",
        " \t" + CleanKey,
        "header.\r\npay-\nload.sign_\tature",
        "\u00a0header.\u2003pay-load.sign_ature\u2028"
    };

    [Theory]
    [MemberData(nameof(KeysWithWhitespace))]
    public void ExplicitKey_StripsWhitespace_AndTakesPrecedence(string key)
    {
        WithEnvironment(autoMapper: "env-automapper-key", shared: "env-shared-key", () =>
            LicenseAccessor.ResolveLicenseKey(key).ShouldBe(CleanKey));
    }

    [Theory]
    [MemberData(nameof(KeysWithWhitespace))]
    public void AutoMapperEnvironmentVariable_StripsWhitespace_AndTakesPrecedence(string key)
    {
        WithEnvironment(autoMapper: key, shared: "env-shared-key", () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBe(CleanKey));
    }

    [Theory]
    [MemberData(nameof(KeysWithWhitespace))]
    public void SharedEnvironmentVariable_StripsWhitespace(string key)
    {
        WithEnvironment(autoMapper: null, shared: key, () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBe(CleanKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("\u00a0\u2003\u2028")]
    public void BlankExplicitKey_FallsBackToAutoMapperEnvironmentVariable(string key)
    {
        WithEnvironment(autoMapper: " env-automapper-key\n", shared: "env-shared-key", () =>
            LicenseAccessor.ResolveLicenseKey(key).ShouldBe("env-automapper-key"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("\u00a0\u2003\u2028")]
    public void BlankHigherPrecedenceKeys_FallBackToSharedEnvironmentVariable(string key)
    {
        WithEnvironment(autoMapper: key, shared: " env-shared-key\n", () =>
            LicenseAccessor.ResolveLicenseKey(key).ShouldBe("env-shared-key"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("\u00a0\u2003\u2028")]
    public void ReturnsNull_WhenAllKeysAreBlank(string key)
    {
        WithEnvironment(autoMapper: key, shared: key, () =>
            LicenseAccessor.ResolveLicenseKey(key).ShouldBeNull());
    }

    [Fact]
    public void ExplicitKey_TakesPrecedence_OverBothEnvironmentVariables()
    {
        const string explicitKey = "explicit-license-key";
        WithEnvironment(autoMapper: "env-automapper-key", shared: "env-shared-key", () =>
            LicenseAccessor.ResolveLicenseKey(explicitKey).ShouldBe(explicitKey));
    }

    [Fact]
    public void AutoMapperEnvironmentVariable_Used_WhenNoExplicitKey()
    {
        const string autoMapperKey = "env-automapper-key";
        WithEnvironment(autoMapper: autoMapperKey, shared: null, () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBe(autoMapperKey));
    }

    [Fact]
    public void SharedEnvironmentVariable_Used_WhenOnlyItIsSet()
    {
        const string sharedKey = "env-shared-key";
        WithEnvironment(autoMapper: null, shared: sharedKey, () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBe(sharedKey));
    }

    [Fact]
    public void AutoMapperEnvironmentVariable_TakesPrecedence_OverSharedEnvironmentVariable()
    {
        const string autoMapperKey = "env-automapper-key";
        WithEnvironment(autoMapper: autoMapperKey, shared: "env-shared-key", () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBe(autoMapperKey));
    }

    [Fact]
    public void ReturnsNull_WhenNothingIsSet()
    {
        WithEnvironment(autoMapper: null, shared: null, () =>
            LicenseAccessor.ResolveLicenseKey(null).ShouldBeNull());
    }

    private static void WithEnvironment(string autoMapper, string shared, Action assert)
    {
        var originalAutoMapper = Environment.GetEnvironmentVariable(AutoMapperEnvVar);
        var originalShared = Environment.GetEnvironmentVariable(SharedEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(AutoMapperEnvVar, autoMapper);
            Environment.SetEnvironmentVariable(SharedEnvVar, shared);
            assert();
        }
        finally
        {
            Environment.SetEnvironmentVariable(AutoMapperEnvVar, originalAutoMapper);
            Environment.SetEnvironmentVariable(SharedEnvVar, originalShared);
        }
    }
}
