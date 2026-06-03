using GroupDocs.Total.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Total.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class ToolDiscoveryTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ToolDiscoveryTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    // The 38 tools Total advertises. Ordered alphabetically inside each domain.
    private static readonly string[] ExpectedTools =
    [
        // Cross-product (2)
        "GetDocumentInfo",
        "GetDocumentPageImage",
        // Annotation (8)
        "AnnotationAddAnnotation",
        "AnnotationAddReply",
        "AnnotationExportAnnotations",
        "AnnotationGetAnnotations",
        "AnnotationImportAnnotations",
        "AnnotationRemoveAnnotations",
        "AnnotationRemoveReplies",
        "AnnotationUpdateAnnotation",
        // Signature (7)
        "SignatureSearchBarcodes",
        "SignatureSearchDigitalSignatures",
        "SignatureSearchImageSignatures",
        "SignatureSearchQrCodes",
        "SignatureSearchTextSignatures",
        "SignatureSign",
        "SignatureVerify",
        // Comparison (1)
        "ComparisonCompare",
        // Conversion (2)
        "ConversionConvert",
        "ConversionGetSupportedFormats",
        // Markdown (2)
        "MarkdownComposeFromMarkdown",
        "MarkdownConvertToMarkdown",
        // Merger (2)
        "MergerMerge",
        "MergerSplit",
        // Metadata (2)
        "MetadataReadMetadata",
        "MetadataRemoveMetadata",
        // Parser (5)
        "ParserExtractBarcodes",
        "ParserExtractImages",
        "ParserExtractMetadata",
        "ParserExtractTables",
        "ParserExtractText",
        // Redaction (4)
        "RedactionEraseMetadata",
        "RedactionRedactAnnotations",
        "RedactionRedactImageArea",
        "RedactionRedactText",
        // Watermark (3)
        "WatermarkAddWatermark",
        "WatermarkRemoveWatermarks",
        "WatermarkSearchWatermarks",
    ];

    [Fact]
    public void ServerInfo_AdvertisesGroupDocsTotalMcp()
    {
        var info = _fixture.Client.ServerInfo;

        Assert.NotNull(info);
        Assert.Equal("GroupDocs.Total.Mcp", info!.Name);
        Assert.False(string.IsNullOrWhiteSpace(info.Version));

        _output.WriteLine($"Server: {info.Name} {info.Version}  (package under test: {_fixture.PackageVersionUnderTest})");
    }

    [Fact]
    public async Task ListTools_ExposesAll38Tools()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        foreach (var tool in catalog.All)
            _output.WriteLine($"tool: {tool.Name}");

        Assert.Equal(38, catalog.All.Count);

        foreach (var expected in ExpectedTools)
        {
            var tool = catalog.ByName(expected);
            Assert.NotNull(tool);
        }
    }

    [Fact]
    public async Task AllTools_HaveNonEmptyDescriptionAndInputSchema()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        Assert.NotEmpty(catalog.All);
        foreach (var tool in catalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Description),
                $"Tool '{tool.Name}' has no description.");

            var schema = tool.JsonSchema;
            Assert.True(schema.ValueKind == System.Text.Json.JsonValueKind.Object,
                $"Tool '{tool.Name}' has no object input schema.");
            Assert.True(schema.TryGetProperty("properties", out _),
                $"Tool '{tool.Name}' schema missing 'properties'.");
        }
    }

    [Theory]
    [InlineData("Annotation",  8)]
    [InlineData("Signature",   7)]
    [InlineData("Comparison",  1)]
    [InlineData("Conversion",  2)]
    [InlineData("Markdown",    2)]
    [InlineData("Merger",      2)]
    [InlineData("Metadata",    2)]
    [InlineData("Parser",      5)]
    [InlineData("Redaction",   4)]
    [InlineData("Watermark",   3)]
    public async Task ListTools_PerProductCount_Matches(string productPrefix, int expectedCount)
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);
        var byProduct = catalog.ForProduct(productPrefix).ToList();

        foreach (var t in byProduct)
            _output.WriteLine($"  {productPrefix}: {t.Name}");

        Assert.Equal(expectedCount, byProduct.Count);
    }
}
