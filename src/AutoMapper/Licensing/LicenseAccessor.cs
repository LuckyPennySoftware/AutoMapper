using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Convert = System.Convert;

namespace AutoMapper.Licensing;

internal class LicenseAccessor
{
    private readonly IGlobalConfiguration _configuration;
    private readonly ILogger _logger;

    public LicenseAccessor(IGlobalConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger("LuckyPennySoftware.AutoMapper.License");
    }

    private License _license;
    private readonly object _lock = new();

    public License Current => _license ??= Initialize();

    private License Initialize()
    {
        lock (_lock)
        {
            if (_license != null)
            {
                return _license;
            }

            var key = _configuration.LicenseKey;
            if (key == null)
            {
                return new License();
            }

            var licenseClaims = ValidateKey(key);
            return licenseClaims.Any()
                ? new License(new ClaimsPrincipal(new ClaimsIdentity(licenseClaims)))
                : new License();
        }
    }

    private Claim[] ValidateKey(string licenseKey)
    {
        try
        {
            var handler = new JsonWebTokenHandler();

            var rsa = new RSAParameters
            {
                Exponent = Convert.FromBase64String("AQAB"),
                Modulus = Convert.FromBase64String(
                    "2LTtdJV2b0mYoRqChRCfcqnbpKvsiCcDYwJ+qPtvQXWXozOhGo02/V0SWMFBdbZHUzpEytIiEcojo7Vbq5mQmt4lg92auyPKsWq6qSmCVZCUuL/kpYqLCit4yUC0YqZfw4H9zLf1yAIOgyXQf1x6g+kscDo1pWAniSl9a9l/LXRVEnGz+OfeUrN/5gzpracGUY6phx6T09UCRuzi4YqqO4VJzL877W0jCW2Q7jMzHxOK04VSjNc22CADuCd34mrFs23R0vVm1DVLYtPGD76/rGOcxO6vmRc7ydBAvt1IoUsrY0vQ2rahp51YPxqqhKPd8nNOomHWblCCA7YUeV3C1Q==")
            };

            var key = new RsaSecurityKey(rsa)
            {
                KeyId = "LuckyPennySoftwareLicenseKey/bbb13acb59904d89b4cb1c85f088ccf9"
            };

            var parms = new TokenValidationParameters
            {
                ValidIssuer = "https://luckypennysoftware.com",
                ValidAudience = "LuckyPennySoftware",
                IssuerSigningKey = key,
                ValidateLifetime = false
            };

            var validateResult = handler.ValidateTokenAsync(licenseKey, parms).Result;
            if (!validateResult.IsValid)
            {
                _logger.LogCritical(validateResult.Exception, "Error validating the Lucky Penny software license key");
            }

            return validateResult.ClaimsIdentity?.Claims.ToArray() ?? [];
        }
        catch (PlatformNotSupportedException)
        {
            _logger.LogInformation(
                "RSA cryptography is not supported on this platform. " +
                "For client redistribution scenarios such as Blazor WASM, see: " +
                "https://docs.automapper.io/en/latest/License-configuration.html#client-redistribution-scenarios");
            return [];
        }
    }

}