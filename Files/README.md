# Files/ — integration test samples

Real document samples used by the per-tool smoke tests. The fixtures
(`SampleDocuments.CopyRealSamples`) stage them into the per-test temp
storage folder; tests skip themselves when a sample is missing, so this
folder may be safely pruned.

## Provenance

The Total upstream repo (`GroupDocs.Total-for-.NET`) ships only demo
applications and no shared SampleFiles folder, so we vendor cross-product
samples from the upstream
[GroupDocs.Annotation for .NET examples repo](https://github.com/groupdocs-annotation/GroupDocs.Annotation-for-.NET)
under `Examples/GroupDocs.Annotation.Examples.CSharp/Resources/SampleFiles/`.
These are generic document fixtures that work across every bundled engine
(Annotation, Comparison, Conversion, Markdown, Merger, Metadata, Parser,
Redaction, Signature, Watermark).

| File                            | Source path                                                  | Used by (Total tools)                                                                 |
|---------------------------------|--------------------------------------------------------------|---------------------------------------------------------------------------------------|
| `input.pdf`                     | `Annotation-for-.NET/.../SampleFiles/input.pdf`              | Most per-tool smoke tests; the universal "any document" fixture                       |
| `input.docx`                    | `Annotation-for-.NET/.../SampleFiles/input.docx`             | Markdown, Conversion, Comparison, Watermark, Metadata smoke tests                     |
| `input.xlsx`                    | `Annotation-for-.NET/.../SampleFiles/input.xlsx`             | Parser table extraction, Conversion, GetDocumentInfo                                  |
| `annotated.pdf`                 | `Annotation-for-.NET/.../SampleFiles/annotated.pdf`          | AnnotationGetAnnotations, AnnotationUpdate/Remove, AnnotationExport                   |
| `annotated.docx`                | `Annotation-for-.NET/.../SampleFiles/annotated.docx`         | AnnotationGetAnnotations on DOCX                                                      |
| `annotated_with_replies.pdf`    | `Annotation-for-.NET/.../SampleFiles/annotated_with_replies.pdf` | AnnotationAddReply, AnnotationRemoveReplies                                       |
| `annotation.xml`                | `Annotation-for-.NET/.../SampleFiles/annotation.xml`         | AnnotationImportAnnotations (XML route)                                               |

## License

The upstream examples repo is published under the MIT license — the same
license as this Tests repo.

## Adding more

If you need a sample for a new tool (e.g. a signed PDF for SignatureVerify,
or a watermarked DOCX for WatermarkRemoveWatermarks):

1. Pick the smallest representative file from the relevant upstream examples
   repo, or generate a minimal valid file.
2. Drop it under `Files/`.
3. Add a `public const string Sample…` to
   `src/GroupDocs.Total.Mcp.Tests/Fixtures/SampleDocuments.cs` and include it
   in `RealSamples`.
4. Reference it from the relevant per-tool test class.
5. Update the provenance table above.
