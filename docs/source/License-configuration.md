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

### License Enforcement

Licensing is enforced via log messages at various levels:

- INFO: Valid license message
- WARNING: Missing license message
- ERROR: Invalid/expired license message

There is no other license enforcement besides log messages. No central license server, no outbound HTTP calls, no degrading or disabling of features.

The log messages are logged using standard `Microsoft.Extensions.Logging` loggers under the category name `LuckyPennySoftware.AutoMapper.License`.

### Multiple License Messages

If you see duplicate license log messages at startup, this is typically caused by AutoMapper being registered or configured more than once in your application.

#### Using `AddAutoMapper` (Microsoft.Extensions.DependencyInjection)

Calling `AddAutoMapper` multiple times in the same `IServiceCollection` is safe. After the first call registers `IMapper`, subsequent calls are ignored and no additional license validation occurs:

```c#
services.AddAutoMapper(cfg => { cfg.LicenseKey = "key"; });
services.AddAutoMapper(cfg => { /* ignored -- IMapper already registered */ });
```

If you are calling `AddAutoMapper` in multiple places (e.g. in modular startup code), each call still only results in a single `MapperConfiguration` singleton and a single license log message.

#### Using `MapperConfiguration` Directly

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

#### Suppressing Duplicate License Messages

If you intentionally have multiple configurations (for example in integration tests or modular scenarios), you can silence duplicate license log messages by filtering the log category:

```csharp
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
```

### Client Redistribution Scenarios

In the case where AutoMapper is used on a client, including:

- Blazor WASM
- WPF/MAUI/Desktop apps
- Redistributed clients

The license key should NOT be set as this would result in secrets transmitted to the client. Instead, omit the license key configuration and mute the license message category name using:

```csharp
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
```

This will depend on your logging setup. A missing/invalid license key does not affect runtime behavior in any way.