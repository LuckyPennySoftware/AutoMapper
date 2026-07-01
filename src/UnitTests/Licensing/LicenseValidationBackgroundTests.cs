using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper.UnitTests.Licensing;

public class LicenseValidationBackgroundTests
{
    // Regression test for #4640: license validation must not run on the construction
    // thread. Building MapperConfiguration under a lazily-built DI singleton holds the
    // container's build lock, and validating there could deadlock the whole app under a
    // cold-start thread-pool starvation. Validation is logging-only, so it is offloaded
    // to a dedicated background thread. Rather than clamp the global thread pool (flaky,
    // process-wide), we assert the property that makes the deadlock impossible: the
    // constructor returns without validating, and validation logs on a *different* thread.
    [Fact]
    public void License_validation_runs_off_the_construction_thread()
    {
        var provider = new ThreadCapturingLoggerProvider();
        var factory = new LoggerFactory();
        factory.AddProvider(provider);

        var constructingThreadId = Environment.CurrentManagedThreadId;

        // A non-empty (junk) key forces the validation path to run; its result is logged,
        // and that logging must happen on a background thread, never on this one.
        _ = new MapperConfiguration(cfg => cfg.LicenseKey = "not-a-real-license-key", factory);

        // The constructor returned without blocking on validation. The license log should
        // arrive shortly, on a thread other than the one that built the configuration.
        provider.LicenseLogged.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        provider.LoggingThreadId.ShouldNotBe(constructingThreadId);
    }

    private sealed class ThreadCapturingLoggerProvider : ILoggerProvider
    {
        public readonly ManualResetEventSlim LicenseLogged = new(false);
        public int LoggingThreadId;

        public ILogger CreateLogger(string categoryName) =>
            categoryName == "LuckyPennySoftware.AutoMapper.License"
                ? new CapturingLogger(this)
                : NullLogger.Instance;

        public void Dispose() => LicenseLogged.Dispose();

        private sealed class CapturingLogger(ThreadCapturingLoggerProvider owner) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                owner.LoggingThreadId = Environment.CurrentManagedThreadId;
                owner.LicenseLogged.Set();
            }
        }
    }
}
