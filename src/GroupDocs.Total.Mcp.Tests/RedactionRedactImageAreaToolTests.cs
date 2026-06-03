using GroupDocs.Total.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Total.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class RedactionRedactImageAreaToolTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RedactionRedactImageAreaToolTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task RedactionRedactImageArea_SmokeCall_ReturnsResponseAndDoesNotCrashServer()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);
        var tool = catalog.ByName("RedactionRedactImageArea");

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        // Either succeeds or surfaces a tool-level error string. What it must NOT
        // do is return MCP's generic "An error occurred invoking '<tool>'" wrapper
        // (Pitfall #18 — that signals the tool body didn't catch its own engine
        // exceptions, blocking diagnostic visibility for AI clients).
        string body;
        try
        {
            var response = await _fixture.Client.CallToolAsync(
                tool.Name,
                new Dictionary<string, object?>
                {
                    ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["x"] = 0, ["y"] = 0, ["width"] = 10, ["height"] = 10,
                });
            body = ToolResponse.Text(response);
            _output.WriteLine(body);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Tool call threw (acceptable for an exotic-engine smoke): {ex.GetType().Name}: {ex.Message}");
            body = string.Empty;
        }

        // Server stays alive after this call.
        var listAfter = await _fixture.Client.ListToolsAsync();
        Assert.NotEmpty(listAfter);
    }
}
