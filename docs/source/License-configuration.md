# Licensing

AutoMapper is [dual licensed](https://github.com/LuckyPennySoftware/AutoMapper/blob/main/LICENSE.md). To configure the commercial license, either the license key can be set using `Microsoft.Extensions.DependencyInjection` integration:

```c#
services.AddAutoMapper(cfg => {
    cfg.LicenseKey = "License key here";

    // Other configuration
});
```

Or on non-MS.Ext.DI scenarios, where you're using AutoMapper directly, you can set the license key in the constructor for the `MappingConfiguration`:

```c#
var mapperConfiguration = new MapperConfiguration(cfg => {
  cfg.LicenseKey = "License Key Here";
}, loggerFactory);
```

You can obtain a valid license from the [AutoMapper website](https://automapper.io).

## Auto-Discovery via Environment Variables

If no license key is set in code, AutoMapper looks for one in environment variables. This is convenient for containerized and cloud environments, and for enterprises that share a single key across many services without code changes:

- `AUTOMAPPER_LICENSE_KEY` – the AutoMapper-specific license key.
- `LUCKYPENNY_LICENSE_KEY` – a shared key usable across Lucky Penny products (for example, [MediatR](https://github.com/LuckyPennySoftware/MediatR) reads the same variable). Because it is shared, the key must be for a license that includes AutoMapper (a `Bundle` or AutoMapper edition); a MediatR-only license will not validate here.

The license key is resolved in the following order of precedence, using the first value found:

1. An explicit value set in code (`cfg.LicenseKey = "..."`).
2. The `AUTOMAPPER_LICENSE_KEY` environment variable.
3. The `LUCKYPENNY_LICENSE_KEY` environment variable.

No code change is required when using an environment variable—just register AutoMapper as usual without setting `LicenseKey`.

## License Enforcement

Licensing is enforced via log messages at various levels:

- INFO: Valid license message, or license validation is not supported on this platform (see Client Redistribution Scenarios below)
- WARNING: Missing license message, or a license term that has ended but is still within its grace period
- ERROR: Invalid/expired license message, or a license key that is incomplete or corrupted
- CRITICAL: The license key could not be validated (see Troubleshooting License Key Validation below)

### Renewal grace period

License keys include a 30-day grace period after the end of their term. When your subscription renews, you receive a new license key, and the previous key keeps logging only a WARNING (not an ERROR) for 30 days after its term ends. This gives you time to deploy the renewed key. Your current license key is always available on your [account page](https://luckypennysoftware.com/account).

There is no other license enforcement besides log messages. No central license server, no outbound HTTP calls, no degrading or disabling of features.

The log messages are logged using standard `Microsoft.Extensions.Logging` loggers under the category name `LuckyPennySoftware.AutoMapper.License`.

## Multiple License Messages

If you see duplicate license log messages at startup, this is typically caused by AutoMapper being registered or configured more than once in your application.

### Using `AddAutoMapper` (Microsoft.Extensions.DependencyInjection)

Calling `AddAutoMapper` multiple times in the same `IServiceCollection` is safe. After the first call registers `IMapper`, subsequent calls are ignored and no additional license validation occurs:

```c#
services.AddAutoMapper(cfg => { cfg.LicenseKey = "key"; });
services.AddAutoMapper(cfg => { /* ignored -- IMapper already registered */ });
```

If you are calling `AddAutoMapper` in multiple places (e.g. in modular startup code), each call still only results in a single `MapperConfiguration` singleton and a single license log message.

### Using `MapperConfiguration` Directly

When creating `MapperConfiguration` instances manually, each instance performs its own license validation, producing a separate log message:

```c#
// Each of these logs a license message – avoid creating multiple instances
var config1 = new MapperConfiguration(cfg => { cfg.LicenseKey = "key"; }, loggerFactory);
var config2 = new MapperConfiguration(cfg => { cfg.LicenseKey = "key"; }, loggerFactory);
```

To avoid duplicate messages, create and reuse a single `MapperConfiguration` instance for the lifetime of your application, typically registered as a singleton:

```c#
// Create once and reuse
var config = new MapperConfiguration(cfg => { cfg.LicenseKey = "key"; }, loggerFactory);
var mapper = new Mapper(config);
```

### Suppressing Duplicate License Messages

If you intentionally have multiple configurations (for example in integration tests or modular scenarios), you can silence duplicate license log messages by filtering the log category:

```csharp
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
```

## Client Redistribution Scenarios

In the case where AutoMapper is used on a client, including:

- Blazor WASM
- WPF/MAUI/Desktop apps
- Redistributed clients

The license key should NOT be set as this would result in secrets transmitted to the client. Instead, omit the license key configuration and mute the license message category name using:

```csharp
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
```

This will depend on your logging setup. A missing/invalid license key does not affect runtime behavior in any way.

If a license key is set on a platform that does not support RSA cryptography, such as Blazor WASM, AutoMapper skips validation and logs an INFO message linking to this section instead of a validation error.

## Troubleshooting License Key Validation

When a license key can't be validated, AutoMapper logs a message describing the problem. The underlying exception is logged separately at `Debug` level; to see it, enable debug logging for the `LuckyPennySoftware.AutoMapper.License` category:

```csharp
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.Debug);
```

### The license key is incomplete or corrupted

The key is missing characters or contains characters that aren't part of a license key. This usually happens when the key is copied from a PDF or an email, where it can be truncated or wrapped across lines.

AutoMapper removes whitespace (spaces, tabs, and line breaks) from the key before validating it, so line breaks and trailing newlines, such as those in Kubernetes secrets created from files, are not a problem. Missing characters can't be recovered: copy the full key again from your [account page](https://luckypennysoftware.com/account).

### The license key signature does not match

The key is well formed but its contents have been changed, usually by a copy/paste or formatting problem. Copy the full key again from your [account page](https://luckypennysoftware.com/account).

Online JWT decoders such as jwt.io can show a damaged key as valid, because they ignore whitespace and don't check the signature against the Lucky Penny Software key. Use your account page as the source of truth.

### The license key is not a Lucky Penny Software license key

The value is a token, but not one issued by Lucky Penny Software. Check that the value you configured (in code or in the `AUTOMAPPER_LICENSE_KEY` or `LUCKYPENNY_LICENSE_KEY` environment variables) is the license key from your [account page](https://luckypennysoftware.com/account).

### Mismatched Microsoft.IdentityModel package versions

AutoMapper validates license keys with the `Microsoft.IdentityModel.JsonWebTokens` package. If your application references different versions of the `Microsoft.IdentityModel.*` packages (for example, through an authentication library that pins an older `Microsoft.IdentityModel.Tokens`), validation fails with an error such as:

```
System.MissingMethodException: Method not found: 'Void Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(...)'
```

To find the versions in use, run:

```
dotnet list package --include-transitive
```

Then reference the same version of every `Microsoft.IdentityModel.*` package, for example by adding direct references at the highest version in use:

```xml
<PackageReference Include="Microsoft.IdentityModel.JsonWebTokens" Version="8.14.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.14.0" />
```

If you use [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management), you can enable transitive pinning (`CentralPackageTransitivePinningEnabled`) and pin the versions in `Directory.Packages.props`.
