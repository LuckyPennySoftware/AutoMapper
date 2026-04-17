using Xunit;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoMapper.UnitTests.Licensing;

public class LicenseKeyEnvironmentVariableTests
{
    #region Environment Variable Auto-Detection Tests

    [Fact]
    public void LicenseKey_ReadsFromEnvironmentVariable_WhenNotExplicitlySet()
    {
        // Arrange
        const string expectedKey = "test-license-key-12345";
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", expectedKey);
        
        try
        {
            var config = new MapperConfigurationExpression();

            // Act
            var actualKey = config.LicenseKey;

            // Assert
            Assert.Equal(expectedKey, actualKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
        }
    }

    [Fact]
    public void LicenseKey_ReturnsNull_WhenEnvironmentVariableNotSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
        var config = new MapperConfigurationExpression();

        // Act
        var actualKey = config.LicenseKey;

        // Assert
        Assert.Null(actualKey);
    }

    #endregion

    #region Backward Compatibility - Old Way Tests

    [Fact]
    public void LicenseKey_SupportsOldWay_DirectAssignment()
    {
        // Arrange
        const string licenseKey = "old-way-explicit-key";
        var config = new MapperConfigurationExpression();

        // Act
        config.LicenseKey = licenseKey;
        var actualKey = config.LicenseKey;

        // Assert
        Assert.Equal(licenseKey, actualKey);
    }

    [Fact]
    public void LicenseKey_PrioritizesExplicitValue_OverEnvironmentVariable()
    {
        // Arrange
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-license-key";
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", envKey);
        
        try
        {
            var config = new MapperConfigurationExpression();

            // Act
            config.LicenseKey = explicitKey;
            var actualKey = config.LicenseKey;

            // Assert
            Assert.Equal(explicitKey, actualKey);
            Assert.NotEqual(envKey, actualKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
        }
    }

    [Fact]
    public void LicenseKey_OldWayOverridesEnvironmentVariable_InConfigAction()
    {
        // Arrange
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-override-key";
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", envKey);
        
        try
        {
            var config = new MapperConfigurationExpression();
            
            // Act - Old way: set it in the config action
            config.LicenseKey = explicitKey;

            // Assert
            Assert.Equal(explicitKey, config.LicenseKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
        }
    }

    #endregion

    #region Integration Tests - Old Way with DI

    [Fact]
    public void AddAutoMapper_SupportsOldWay_ExplicitLicenseKey()
    {
        // Arrange
        const string licenseKey = "old-way-integration-key";
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => 
        {
            cfg.LicenseKey = licenseKey;
            cfg.CreateMap<TestSource, TestDestination>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        // Act
        var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void AddAutoMapper_UsesEnvironmentVariable_WhenNoExplicitKeySet()
    {
        // Arrange
        const string licenseKey = "env-integration-key";
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", licenseKey);
        
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => 
            {
                // No explicit LicenseKey assignment - should use env var
                cfg.CreateMap<TestSource, TestDestination>();
            });

            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IMapper>();

            // Act
            var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
        }
    }

    [Fact]
    public void AddAutoMapper_OldWayTakesPrecedence_OverEnvironmentVariable()
    {
        // Arrange
        const string envKey = "env-license-key";
        const string explicitKey = "explicit-license-key-from-old-way";
        Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", envKey);
        
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => 
            {
                cfg.LicenseKey = explicitKey;
                cfg.CreateMap<TestSource, TestDestination>();
            });

            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IMapper>();

            // Act
            var result = mapper.Map<TestDestination>(new TestSource { Name = "Test" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", null);
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