using Kid.Mcp;

namespace Kid.Mcp.Tests;

public sealed class SensitiveContentRedactorTests
{
    [Theory]
    [InlineData("https://example.test/callback?code=abc123&state=xyz", "https://example.test/callback?code=[REDACTED]&state=[REDACTED]")]
    [InlineData("token=abc123", "token=[REDACTED]")]
    [InlineData("https://example.test/?q=public#access_token=secret", "https://example.test/?q=public#access_token=[REDACTED]")]
    public void RedactMasksSensitiveUrlParameters(string input, string expected)
    {
        Assert.Equal(expected, SensitiveContentRedactor.Redact(input));
    }

    [Fact]
    public void RedactKeepsOrdinaryContentAndNull()
    {
        Assert.Equal("Search results for public text", SensitiveContentRedactor.Redact("Search results for public text"));
        Assert.Null(SensitiveContentRedactor.Redact(null));
    }
}
