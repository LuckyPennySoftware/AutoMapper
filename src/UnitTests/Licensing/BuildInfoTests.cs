using AutoMapper.Licensing;

namespace AutoMapper.UnitTests.Licensing;

public class BuildInfoTests
{
    // Perpetual licensing depends on the build date embedded by the EmbedBuildDate target
    [Fact]
    public void BuildDate_IsEmbedded() => BuildInfo.BuildDate.ShouldNotBeNull();
}
