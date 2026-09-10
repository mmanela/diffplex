# DiffPlex benchmark results

Benchmark runs use the `DiffPlex.Benchmarks` project:

```console
dotnet run -c Release --project DiffPlex.Benchmarks -- --exporters markdown,csv
```

The harness uses BenchmarkDotNet with `MemoryDiagnoser`, one launch, one warmup iteration, and three measured iterations on .NET 8. Results below were captured on the GitHub-hosted coding-agent runner for this PR, so they are intended for relative before/after comparisons rather than absolute machine-independent numbers.

## Baseline

Baseline results will be recorded after the benchmark harness is added and before library optimizations are applied.

## Final comparison

Final results will be recorded after the optimization pass.
