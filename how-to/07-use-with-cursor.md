# Use with Cursor

Connect the GroupDocs.Total MCP server to [Cursor](https://cursor.com) so you can
ask its Agent to annotate, sign, convert, compare, watermark, parse, redact, or
inspect documents — all 38 tools across 10 product domains from one endpoint.

> **Distribution note.** The `GroupDocs.Total.Mcp` NuGet package bundles every
> GroupDocs engine, including GroupDocs.Parser's ~234 MiB of embedded ONNX
> models. The packed nupkg is ~393 MiB — over NuGet.org's 250 MB limit — so
> **Total is distributed Docker-first** (`ghcr.io/groupdocs-total/total-net-mcp`).
> Prefer **Option C (Docker)** below. The `dnx` route is documented for parity
> and only works if/when a size-trimmed package is published to NuGet.

## Prerequisites

- Cursor installed and updated (MCP support is in **Settings → Tools & MCP**).
- One of:
  - [Docker](https://www.docker.com/products/docker-desktop) (recommended for Total), or
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for the `dnx` route — only if a NuGet-published build exists).

## Config file location

Cursor uses the **`mcpServers`** key (like Claude Desktop) — **not** `servers`
as in VS Code. Two scopes:

| Scope | Path |
|---|---|
| Global (all projects) | `~/.cursor/mcp.json` (macOS/Linux) · `%USERPROFILE%\.cursor\mcp.json` (Windows) |
| Project-only | `.cursor/mcp.json` in the workspace root |

Create the file if it doesn't exist.

## Option C — Docker (recommended for Total)

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/you/Documents:/data",
        "ghcr.io/groupdocs-total/total-net-mcp:26.7.0"
      ],
      "env": {
        "GROUPDOCS_LICENSE_PATH": "/data/GroupDocs.Total.lic"
      }
    }
  }
}
```

- Bind an **absolute host path** to `/data`; the server reads/writes files there.
  On Windows use `"C:\\Users\\you\\Documents:/data"`.
- Drop the `GROUPDOCS_LICENSE_PATH` env line to run in evaluation mode (read-only
  tools work; tools that call `Save()` produce watermarked output or are blocked).
- Omit `:26.7.0` for `:latest`.

Copy-paste starter: [examples/cursor-mcp.json](../examples/cursor-mcp.json).

## Option A — dnx (only if a NuGet build is published)

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "command": "dnx",
      "args": ["GroupDocs.Total.Mcp@26.7.0", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

- Replace the storage path with an **absolute path** to the folder Cursor should
  operate on. On Windows use `"C:\\Users\\you\\Documents"` (double-escaped) or
  forward slashes.
- Add `"GROUPDOCS_LICENSE_PATH": "…/GroupDocs.Total.lic"` to `env` to unlock the
  tools that call `Save()` (Conversion, Watermark, Merger, Redaction, Signature
  sign, …), which are watermarked/blocked in evaluation mode.

## Option B — Windows: full path to `dotnet.exe` (SSL / timeout workaround)

On Windows, Cursor launching `dnx` can fail with an **SSL / ~30 s timeout** on
the first package probe. Bypass `dnx` by running the already-cached tool DLL
directly with `dotnet.exe`:

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "command": "C:\\Program Files\\dotnet\\dotnet.exe",
      "args": [
        "C:\\Users\\you\\.nuget\\packages\\groupdocs.total.mcp\\26.7.0\\tools\\net10.0\\any\\GroupDocs.Total.Mcp.dll"
      ],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "C:\\Users\\you\\Documents"
      }
    }
  }
}
```

Populate the cache first by running `dnx GroupDocs.Total.Mcp@26.7.0 --yes` once
in a terminal, then point `args[0]` at the resulting
`…\.nuget\packages\groupdocs.total.mcp\<version>\tools\net10.0\any\GroupDocs.Total.Mcp.dll`.

## Reload and verify

1. Save `mcp.json`.
2. **Settings → Tools & MCP** → find `groupdocs-total` → toggle it on (or hit
   the reload icon). A green dot means it connected.
3. Expand it — you should see the 38 tools, e.g. `AnnotationAddAnnotation`,
   `SignatureSign`, `ComparisonCompare`, `ConversionConvert`, `MergerMerge`,
   `MetadataReadMetadata`, `ParserExtractText`, `RedactionRedactText`,
   `WatermarkAddWatermark`, plus cross-product `GetDocumentInfo` and
   `GetDocumentPageImage`.

## Example prompts (Agent mode)

```
What kind of document is contract.pdf, and how many pages does it have?

Compare source.docx and target.docx and tell me what changed.

Convert report.docx to PDF.

Add a "CONFIDENTIAL" watermark to every page of report.pdf.

Redact all email addresses from statement.pdf.
```

The Agent picks the right `{Product}{Verb}{Noun}` tool from the unified catalog
and composes its answer from the results.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Server greyed out / won't start on Windows (dnx route) | `dnx` SSL/timeout — use **Option B** (full `dotnet.exe` path + cached DLL), or switch to **Option C (Docker)**. |
| `dnx` can't find the package | Expected — `GroupDocs.Total.Mcp` is Docker-first (nupkg exceeds NuGet's 250 MB limit). Use **Option C (Docker)**. |
| Server not listed | JSON typo — Cursor silently drops unparseable entries. Validate with `jq . mcp.json`. Confirm the key is `mcpServers`, not `servers`. |
| Tools that write output fail or watermark | Evaluation mode. Add `GROUPDOCS_LICENSE_PATH` pointing at a `GroupDocs.Total.lic`. |
| `DllNotFoundException: libgdiplus` (macOS/Linux, dnx route) | Install native deps — `brew install mono-libgdiplus` (macOS) / `apt-get install libgdiplus libfontconfig1` (Linux), or use the Docker option. |

## Next steps

- [04 — Use with Claude Desktop](04-use-with-claude-desktop.md)
- [05 — Use with VS Code / Copilot](05-use-with-vscode-copilot.md)
