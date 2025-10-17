AutoMapper is [dual licensed](https://github.com/LuckyPennySoftware/AutoMapper/blob/master/LICENSE.md). To configure the commercial license, either the license key can be set using `Microsoft.Extensions.DependencyInjection` integration:

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