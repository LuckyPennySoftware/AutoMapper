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
