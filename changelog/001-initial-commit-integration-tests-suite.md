---
id: 001
date: 2026-05-31
package-under-test: 26.5.0
type: feature
---

# Initial integration test suite for GroupDocs.Total.Mcp

## What changed

- xUnit test project targeting `net10.0`, referencing only the published
  `ModelContextProtocol` 1.1.0 NuGet — no project reference to the server source.
- `McpServerFixture` launches the published `GroupDocs.Total.Mcp@26.5.0` package
  via `dnx` as a child process, wires an MCP stdio client, and seeds a temporary
  storage folder with synthetic + real sample documents.
- `SampleDocuments` builds a minimal blank PDF and a baseline 1×1 JPEG from
  byte arrays at runtime, and copies cross-product real samples from `Files/`
  (committed `input.pdf` / `input.docx` / `input.xlsx` / `annotated.pdf` /
  `annotated.docx` / `annotated_with_replies.pdf` / `annotation.xml` —
  see [Files/README.md](../Files/README.md) for provenance, vendored from
  the upstream GroupDocs.Annotation-for-.NET examples repo because the
  Total upstream repo ships only demo apps with no shared SampleFiles
  folder).
- `ToolCatalog` does exact-name lookup with PascalCase + snake_case fallback —
  Total exposes 38 tools using `{Product}{Verb}{Noun}` naming and the
  keyword-substring pattern used by single-product MCPs would collide across
  domains.
- Forty test classes covering all 38 tools exposed by the server:
  - `ToolDiscoveryTests` (13 tests = 3 fixed + 10 per-product count theory):
    server info, asserts `Equal(38, catalog.All.Count)`, name-checks each
    of the 38 expected tools, asserts per-product counts
    (Annotation=8, Signature=7, Comparison=1, Conversion=2, Markdown=2,
    Merger=2, Metadata=2, Parser=5, Redaction=4, Watermark=3).
  - `ErrorHandlingTests` (3) — unknown file, corrupted bytes, password parameter,
    all routed through the cross-product `GetDocumentInfo` tool.
  - 38 per-tool smoke tests — one `<ToolName>Tests.cs` per advertised tool.
    Each calls the tool with sensible default args and asserts the server
    stays up. The tests are deliberately lenient on success/failure of the
    individual engine — they primarily exercise the dispatch surface and
    Pitfall #18 wrapping. `MarkdownComposeFromMarkdownTests` is hand-written
    because that tool has no positional `file` parameter (it takes
    `outputFileName` + inline `markdown` text / `sourceFile`).
- GitHub Actions workflow `.github/workflows/integration.yml`:
  - Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`.
  - Linux step installs `libgdiplus` + `libfontconfig1` + `ttf-mscorefonts-installer`
    because Annotation / Watermark / Signature / Viewer / Conversion /
    Comparison engines all rasterise pages via System.Drawing.
  - macOS step `brew install mono-libgdiplus` and copies `libgdiplus.dylib`
    into the .NET shared-framework directory so dnx's child process can
    `dlopen` it.
  - Triggers: push, PR, nightly cron, `workflow_dispatch` (with `package_version`
    input), `repository_dispatch` (`nuget-published` event for release smoke).
  - Optional `GROUPDOCS_LICENSE` repo secret auto-decoded into `$RUNNER_TEMP`
    and exported as `GROUPDOCS_LICENSE_PATH` to drop the eval-mode watermark
    on write-path tools.
- `examples/` — ready-to-use `claude-desktop.json`, `vscode-mcp.json`,
  `docker-compose.yml` copy-paste configs.
- `AGENTS.md` + `llms.txt` for AI coding agent orientation.
- `how-to/` guides covering every deployment channel.

## Why

Closes the release-validation gap: the main repo's unit tests mock
`IFileResolver` / `ILicenseManager` and validate tool dispatch, but nothing
previously exercised the **shipped** Total NuGet end-to-end. Every release
now has a cross-platform smoke check against live nuget.org before users
hit it.

## Migration / impact

First release of this repository — no migration. To wire the release-smoke
trigger, add a `gh api repos/.../dispatches -f event_type=nuget-published -f
'client_payload[package_version]=…'` step to the main repo's publish workflow
after `dotnet nuget push` succeeds. See `how-to/06-run-integration-tests.md`.

Known limitation propagated from the main repo: the main `GroupDocs.Total.Mcp`
nupkg lands at ~300 MB after RID-strip optimisation, exceeding NuGet.org's
250 MB free-tier limit. The Aspose org has a higher quota with NuGet.org
(the underlying `GroupDocs.Total` metapackage itself is 311 MB on NuGet), so
publishing should succeed; if not, fallback is Docker-only distribution.
This is an architectural property of bundling 14 engines in one assembly,
not something we can shrink further without restructuring to individual
per-product engine references.
