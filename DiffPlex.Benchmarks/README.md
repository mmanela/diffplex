# DiffPlex benchmarks

This project contains BenchmarkDotNet benchmarks for the core DiffPlex diffing APIs and builders. The scenarios cover identical inputs, many small edits, a few large edits, completely different inputs, and inputs with long common subsequences at small and large sizes.

Run the full suite from this directory:

```console
dotnet run -c Release
```

Useful shorter commands while iterating:

```console
dotnet run -c Release -- --filter "*CreateLineDiffs*"
dotnet run -c Release -- --filter "*Large*" --exporters markdown,csv
```

BenchmarkDotNet writes detailed reports under `BenchmarkDotNet.Artifacts/results` by default. Committed before/after summaries for this optimization effort are tracked in `../benchmarks/RESULTS.md`.
