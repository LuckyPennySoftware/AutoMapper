using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.UnitTests.Licensing;

public class LicenseKeyEnvironmentVariableTests
{
    private const string EnvVarName = "AUTOMAPPER_LICENSE_KEY";

    #region Environment Variable Auto-Detection Tests

    [Fact]
    public void LicenseKey_ReadsFromEnvironmentVariable_WhenNotExplicitlySet()
    {
        const string expectedKey = "test-license-key-12345";
        Environment.SetEnvironmentVariable(EnvVarName, expectedKey);

        try
        {
            var config = new MapperConfigurationExpression();

            config.LicenseKey.ShouldBe(expectedKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvVarName, null);
        }
    }

    [Fact]
    public void LicenseKey_ReturnsNull_WhenEnvironmentVariableNotSet()
    {
        Environment.SetEnvironmentVariable(EnvVarName, null);

        var config = new MapperConfigurationExpression();

        config.LicenseKey.ShouldBeNull();
    }

    #endregion

    #region Backward Compatibility - Old Way Tests

    [Fact]
    public void LicenseKey_SupportsOldWay_DirectAssignment()
    {
        const string licenseKey = "old-way-explicit-key";
        var config = new MapperConfigurationExpression();

        config.LicenseKey = licenseKey;

        config.LicenseKey.ShouldBe(licenseKey);
    }

    [Fact]
    public void LicenseKey_PrioritizesExplicitValue_OverEnvironmentVariable()
    {
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-license-key";
        Environment.SetEnvironmentVariable(EnvVarName, envKey);

        try
        {
            var config = new MapperConfigurationExpression();

            config.LicenseKey = explicitKey;

            config.LicenseKey.ShouldBe(explicitKey);
            config.LicenseKey.ShouldNotBe(envKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvVarName, null);
        }
    }

    [Fact]
    public void LicenseKey_OldWayOverridesEnvironmentVariable_InConfigAction()
    {
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-override-key";
        Environment.SetEnvironmentVariable(EnvVarName, envKey);

        try
        {
            var config = new MapperConfigurationExpression();

            config.LicenseKey = explicitKey;

            config.LicenseKey.ShouldBe(explicitKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvVarName, null);
        }
    }

    #endregion

    #region Integration Tests - Old Way with DI

    [Fact]
    public void AddAutoMapper_SupportsOldWay_ExplicitLicenseKey()
    {
        const string licenseKey = "old-way-integration-key";
        MapperConfigurationExpression capturedCfg = null;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = licenseKey;
            cfg.CreateMap<TestSource, TestDestination>();
            capturedCfg = (MapperConfigurationExpression)cfg;
        });

        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test");
        capturedCfg.LicenseKey.ShouldBe(licenseKey);
    }

    [Fact]
    public void AddAutoMapper_UsesEnvironmentVariable_WhenNoExplicitKeySet()
    {
        const string licenseKey = "env-integration-key";
        Environment.SetEnvironmentVariable(EnvVarName, licenseKey);
        MapperConfigurationExpression capturedCfg = null;

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<TestSource, TestDestination>();
                capturedCfg = (MapperConfigurationExpression)cfg;
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test");
            capturedCfg.LicenseKey.ShouldBe(licenseKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvVarName, null);
        }
    }

    [Fact]
    public void AddAutoMapper_OldWayTakesPrecedence_OverEnvironmentVariable()
    {
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-license-key-from-old-way";
        Environment.SetEnvironmentVariable(EnvVarName, envKey);
        MapperConfigurationExpression capturedCfg = null;

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = explicitKey;
                cfg.CreateMap<TestSource, TestDestination>();
                capturedCfg = (MapperConfigurationExpression)cfg;
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test");
            capturedCfg.LicenseKey.ShouldBe(explicitKey);
            capturedCfg.LicenseKey.ShouldNotBe(envKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvVarName, null);
        }
    }

    #endregion

    #region Test Helper Classes

    private class TestSource
    {
        public string Name { get; set; }
    }

    private class TestDestination
    {
        public string Name { get; set; }
    }

    #endregion
}