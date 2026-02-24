using System.Security.Claims;
using AutoMapper.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using License = AutoMapper.Licensing.License;

namespace AutoMapper.UnitTests.Licensing;

public class LicenseValidatorTests
{
    [Fact]
    public void Should_return_invalid_when_no_claims()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory);
        var license = new License();
        
        license.IsConfigured.ShouldBeFalse();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
     
        logMessages
            .ShouldContain(log => log.Level == LogLevel.Warning);
    }   
    
        
    [Fact]
    public void Should_return_valid_when_community()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Community)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.Bundle)));
        
        license.IsConfigured.ShouldBeTrue();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
     
        logMessages.ShouldNotContain(log => log.Level == LogLevel.Error 
                                            || log.Level == LogLevel.Warning
                                            || log.Level == LogLevel.Critical);
    }
    
    [Fact]
    public void Should_return_invalid_when_not_correct_type()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Professional)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.MediatR)));
        
        license.IsConfigured.ShouldBeTrue();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
     
        logMessages
            .ShouldContain(log => log.Level == LogLevel.Error);
    }
    
    [Fact]
    public void Should_return_invalid_when_expired()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Professional)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.AutoMapper)));
        
        license.IsConfigured.ShouldBeTrue();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
     
        logMessages
            .ShouldContain(log => log.Level == LogLevel.Error);
    }

    [Fact]
    public void Should_allow_perpetual_license_when_build_date_before_expiration()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var buildDate = DateTimeOffset.UtcNow.AddDays(-30);
        var licenseValidator = new LicenseValidator(factory, buildDate);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Professional)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.AutoMapper)),
            new Claim("perpetual", "true"));
        
        license.IsConfigured.ShouldBeTrue();
        license.IsPerpetual.ShouldBeTrue();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
        logMessages.ShouldNotContain(log => log.Level == LogLevel.Error);
        logMessages.ShouldContain(log => log.Level == LogLevel.Information && 
                                         log.Message.Contains("perpetual"));
    }

    [Fact]
    public void Should_reject_perpetual_license_when_build_date_after_expiration()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var buildDate = DateTimeOffset.UtcNow.AddDays(30); // Build date in future (after expiration)
        var licenseValidator = new LicenseValidator(factory, buildDate);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Professional)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.AutoMapper)),
            new Claim("perpetual", "true"));
        
        license.IsConfigured.ShouldBeTrue();
        license.IsPerpetual.ShouldBeTrue();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
        logMessages.ShouldContain(log => log.Level == LogLevel.Error && 
                                        log.Message.Contains("expired"));
    }

    [Fact]
    public void Should_warn_and_error_when_perpetual_but_build_date_is_null()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory, buildDate: null);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeSeconds().ToString()),
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(-10).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Professional)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.AutoMapper)),
            new Claim("perpetual", "true"));

        license.IsConfigured.ShouldBeTrue();
        license.IsPerpetual.ShouldBeTrue();

        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
        logMessages.ShouldContain(log => log.Level == LogLevel.Warning && log.Message.Contains("perpetual"));
        logMessages.ShouldContain(log => log.Level == LogLevel.Error && log.Message.Contains("expired"));
    }

    [Fact]
    public void Should_handle_missing_perpetual_claim()
    {
        var factory = new LoggerFactory();
        var provider = new FakeLoggerProvider();
        factory.AddProvider(provider);

        var licenseValidator = new LicenseValidator(factory);
        var license = new License(
            new Claim("account_id", Guid.NewGuid().ToString()),
            new Claim("customer_id", Guid.NewGuid().ToString()),
            new Claim("sub_id", Guid.NewGuid().ToString()),
            new Claim("iat", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds().ToString()), 
            new Claim("exp", DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString()),
            new Claim("edition", nameof(Edition.Community)),
            new Claim("type", nameof(AutoMapper.Licensing.ProductType.Bundle)));
        
        license.IsConfigured.ShouldBeTrue();
        license.IsPerpetual.ShouldBeFalse();
        
        licenseValidator.Validate(license);

        var logMessages = provider.Collector.GetSnapshot();
        logMessages.ShouldNotContain(log => log.Level == LogLevel.Error 
                                            || log.Level == LogLevel.Warning
                                            || log.Level == LogLevel.Critical);
    }
}