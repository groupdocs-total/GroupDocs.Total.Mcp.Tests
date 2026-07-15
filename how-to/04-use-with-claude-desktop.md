# Use with Claude Desktop

Connect the MCP server to Claude Desktop (macOS / Windows) so you can ask
Claude to annotate, sign, convert, watermark, parse, redact and otherwise process documents end-to-end.

## Prerequisites

- [Claude Desktop](https://claude.ai/download) installed and logged in.
- One of:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for the `dnx` route — recommended), or
  - [Docker](https://www.docker.com/products/docker-desktop) (for the container route).

## Config file location

| OS | Path |
|---|---|
| macOS | `~/Library/Application Support/Claude/claude_desktop_config.json` |
| Windows | `%APPDATA%\Claude\claude_desktop_config.json` |

Create the file if it doesn't exist.

## Option A — dnx (recommended)

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Total.Mcp@26.7.0", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

- Replace `/Users/you/Documents` with an **absolute path** to the folder
  containing documents you want Claude to operate on.
- On Windows use `"C:\\Users\\you\\Documents"` (double-escaped backslashes) or
  forward slashes: `"C:/Users/you/Documents"`.

Full example: [examples/claude-desktop.json](../examples/claude-desktop.json).

### If Claude can't find `dnx`

Claude Desktop launches child processes with a minimal PATH — `dnx` may not be
found on macOS even though it works in your shell. Use the absolute path:

```json
"command": "/usr/local/share/dotnet/dnx"
```

On Windows:

```json
"command": "C:\\Program Files\\dotnet\\dnx.cmd"
```

Find the correct path with:

```bash
which dnx            # macOS / Linux
where dnx.cmd        # Windows (from cmd)
```

## Option B — Docker

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "type": "stdio",
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/you/Documents:/data",
        "ghcr.io/groupdocs-total/total-net-mcp:26.7.0"
      ]
    }
  }
}
```

This works even if you don't have the .NET SDK installed. The first invocation
pulls the image; subsequent launches are fast.

## Option C — Global dotnet tool

```json
{
  "mcpServers": {
    "groupdocs-total": {
      "type": "stdio",
      "command": "groupdocs-total-mcp",
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

Requires you've already run `dotnet tool install -g GroupDocs.Total.Mcp`
(see [01 — NuGet install](01-install-from-nuget.md)).

## Restart Claude Desktop

After editing the config, fully quit and reopen Claude Desktop. On macOS,
`Cmd+Q` — closing the window isn't enough.

## Verify the connection

1. Open a new conversation.
2. Click the **🔨 tools** icon in the composer — you should see all 10 tools
   (`add_annotation`, `get_annotations`, `update_annotation`,
   `remove_annotations`, `add_reply`, `remove_replies`, `import_annotations`,
   `export_annotations`, `get_document_info`, `generate_pages_preview`)
   listed under `groupdocs-total`.
3. If the icon shows an error badge, hover for the details. The most common
   issue is a bad `command` path or invalid `GROUPDOCS_MCP_STORAGE_PATH`.

## Example prompts

```
Convert contract.docx to PDF, then sign it with a QR code
containing "Signed by Alice", then watermark every page with "CONFIDENTIAL".

Compare old.pdf and new.pdf, list every annotation in the result, and preview page 1.

Extract all tables from report.xlsx as Markdown and redact every email address.


Read the metadata of every PDF in /uploads, then strip it, then preview the cleaned page 1 of each.

Find every barcode in invoice.pdf, then merge invoice.pdf + receipt.pdf into combined.pdf.

How many pages does contract.pdf have, and what are its page dimensions?
```

Claude will call the matching tools (`add_annotation`, `get_annotations`,
`generate_pages_preview`, …) and compose its answer from the tool results.

## License note

All 10 tools work in evaluation mode. The write paths (`add_annotation`,
`update_annotation`, `remove_annotations`, `add_reply`, `remove_replies`,
`import_annotations`, `generate_pages_preview`) add a diagnostic
`"[Evaluation mode]"` prefix and may include a watermark on the saved output;
the read-only tools (`get_annotations`, `export_annotations`,
`get_document_info`) are unaffected. To drop the watermark, add the license
path to your config:

```json
"env": {
  "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents",
  "GROUPDOCS_LICENSE_PATH": "/Users/you/.secrets/GroupDocs.Total.lic"
}
```

## Troubleshooting

| Symptom | Fix |
|---|---|
| Server not listed in tools icon | Config JSON has a typo — Claude silently drops unparseable entries. Run it through `jq . claude_desktop_config.json`. |
| Server listed but greyed out | Claude couldn't launch the process. Check `~/Library/Logs/Claude/mcp*.log` on macOS or `%APPDATA%\Claude\logs\mcp*.log` on Windows for stderr from the server. |
| "No license configured" warnings | Expected in evaluation mode — all 38 tools still work; rendered output may have a watermark for write-path tools (Sign / Add* / Convert / Compose / Merge / Remove / Redact). |
| `[Evaluation mode] Output may include watermarks.` | Total write-path tool without a license. Set `GROUPDOCS_LICENSE_PATH` for clean output. |

## Next steps

- [05 — Use with VS Code / Copilot](05-use-with-vscode-copilot.md)
- [03 — MCP registry](03-verify-mcp-registry.md) — confirm the snippet matches what's on nuget.org
