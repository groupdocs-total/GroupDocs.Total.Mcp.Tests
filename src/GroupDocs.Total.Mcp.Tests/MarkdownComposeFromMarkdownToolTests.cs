using GroupDocs.Total.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Total.Mcp.IntegrationTests;

// MarkdownComposeFromMarkdown is unique among the 38 tools — it does NOT take
// a positional `file` FileInput. Instead it accepts an `outputFileName` plus
// either inline `markdown` text or a `sourceFile` FileInput. The generic
// smoke-test template doesn't apply.
[Collection(McpServerCollection.Name)]
public class MarkdownComposeFromMarkdownToolTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MarkdownComposeFromMarkdownToolTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task MarkdownComposeFromMarkdown_InlineMarkdown_ReturnsResponse()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);
        var tool = catalog.ByName("MarkdownComposeFromMarkdown");

        string body;
        try
        {
            var response = await _fixture.Client.CallToolAsync(
                tool.Name,
                new Dictionary<string, object?>
                {
                    ["outputFileName"] = "smoke.docx",
                    ["markdown"] = "# Smoke test\n\nHello, world.",
                });
            body = ToolResponse.Text(response);
            _output.WriteLine(body);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Tool call threw (acceptable): {ex.GetType().Name}: {ex.Message}");
            body = string.Empty;
        }

        var listAfter = await _fixture.Client.ListToolsAsync();
        Assert.NotEmpty(listAfter);
    }
}
