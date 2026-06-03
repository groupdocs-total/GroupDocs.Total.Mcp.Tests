using ModelContextProtocol.Client;

namespace GroupDocs.Total.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by exact name. The Total MCP exposes 38 tools using
/// `{Product}{Verb}{Noun}` PascalCase naming, so the simple "keyword substring"
/// pattern used by single-product MCPs would collide across domains
/// (`add_annotation` vs `add_reply` vs `add_watermark`). Here we pin to the
/// exact tool names declared in the server source, tolerating both PascalCase
/// and snake_case wire conventions.
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;
    private readonly Dictionary<string, McpClientTool> _byName;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools)
    {
        _tools = tools;
        _byName = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;
    public IReadOnlyList<string> Names => _tools.Select(t => t.Name).ToList();

    /// Exact-name lookup. Tries PascalCase first, then falls back to snake_case
    /// since MCP SDKs differ on which casing they expose on the wire.
    public McpClientTool ByName(string name)
    {
        if (_byName.TryGetValue(name, out var tool))
            return tool;

        var snake = ToSnakeCase(name);
        if (_byName.TryGetValue(snake, out tool))
            return tool;

        throw new InvalidOperationException(
            $"No tool named '{name}' (or '{snake}'). Found: {string.Join(", ", _tools.Select(t => t.Name))}");
    }

    /// Filter tools by product-prefix substring (case-insensitive). Cross-product
    /// tools (GetDocumentInfo / GetDocumentPageImage) have no product prefix and
    /// won't match any product filter.
    public IEnumerable<McpClientTool> ForProduct(string productPrefix) =>
        _tools.Where(t => t.Name.StartsWith(productPrefix, StringComparison.OrdinalIgnoreCase)
                       || t.Name.StartsWith(ToSnakeCase(productPrefix), StringComparison.OrdinalIgnoreCase));

    private static string ToSnakeCase(string pascal)
    {
        if (string.IsNullOrEmpty(pascal)) return pascal;
        var sb = new System.Text.StringBuilder(pascal.Length + 8);
        for (int i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (i > 0 && char.IsUpper(c)) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
