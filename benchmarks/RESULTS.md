# DiffPlex benchmark results

Benchmark runs use the `DiffPlex.Benchmarks` project:

```console
dotnet run -c Release --project DiffPlex.Benchmarks -- --filter "*" --exporters github csv
```

The harness uses BenchmarkDotNet with `MemoryDiagnoser`, one launch, one warmup iteration, and three measured iterations on .NET 8. Results below were captured on the GitHub-hosted coding-agent runner for this PR, so they are intended for relative before/after comparisons rather than absolute machine-independent numbers.

## Baseline

Captured before library optimizations in this PR.

```

BenchmarkDotNet v0.13.12, Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 9.0.317
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  Job-ZSKIMK : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Runtime=.NET 8.0  IterationCount=3  LaunchCount=1  
WarmupCount=1  

```
| Method                   | Scenario             | Size  | Mean         | Error        | StdDev     | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated   | Alloc Ratio |
|------------------------- |--------------------- |------ |-------------:|-------------:|-----------:|------:|--------:|---------:|---------:|---------:|------------:|------------:|
| CreateLineDiffs          | Identical            | Small |     13.62 μs |     3.023 μs |   0.166 μs |  1.00 |    0.01 |   0.6409 |   0.0153 |        - |    10.48 KB |        1.01 |
| CreateDiffs_LineChunker  | Identical            | Small |     13.67 μs |     2.448 μs |   0.134 μs |  1.00 |    0.00 |   0.6256 |   0.0153 |        - |    10.41 KB |        1.00 |
| BuildInlineDiffModel     | Identical            | Small |     14.19 μs |     0.803 μs |   0.044 μs |  1.04 |    0.01 |   0.7935 |   0.0305 |        - |    12.98 KB |        1.25 |
| CreateCharacterDiffs     | Identical            | Small |     14.39 μs |     1.013 μs |   0.056 μs |  1.05 |    0.01 |   1.4191 |   0.0916 |        - |    23.34 KB |        2.24 |
| BuildSideBySideDiffModel | Identical            | Small |     14.42 μs |     1.535 μs |   0.084 μs |  1.05 |    0.01 |   0.9613 |   0.0458 |        - |    15.73 KB |        1.51 |
| CreateWordDiffs          | Identical            | Small |     39.12 μs |     8.579 μs |   0.470 μs |  2.86 |    0.03 |   3.1738 |   0.3662 |        - |    52.08 KB |        5.00 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateCharacterDiffs     | Identical            | Large |     55.88 μs |     9.384 μs |   0.514 μs |  0.07 |    0.00 |   5.4321 |   1.0376 |        - |    89.25 KB |        0.15 |
| CreateLineDiffs          | Identical            | Large |    768.69 μs |    55.490 μs |   3.042 μs |  0.94 |    0.00 |  35.1563 |  24.4141 |        - |   582.18 KB |        1.00 |
| BuildInlineDiffModel     | Identical            | Large |    811.80 μs |    41.993 μs |   2.302 μs |  0.99 |    0.01 |  43.9453 |  28.3203 |        - |   731.56 KB |        1.26 |
| CreateDiffs_LineChunker  | Identical            | Large |    821.73 μs |   131.944 μs |   7.232 μs |  1.00 |    0.00 |  35.1563 |  22.4609 |        - |   582.11 KB |        1.00 |
| BuildSideBySideDiffModel | Identical            | Large |    874.99 μs |   133.118 μs |   7.297 μs |  1.06 |    0.00 |  53.7109 |  36.1328 |        - |   881.17 KB |        1.51 |
| CreateWordDiffs          | Identical            | Large |  2,980.90 μs |   394.108 μs |  21.602 μs |  3.63 |    0.01 | 371.0938 | 371.0938 | 371.0938 |  3081.63 KB |        5.29 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateDiffs_LineChunker  | ManySmallChanges     | Small |     13.11 μs |     2.671 μs |   0.146 μs |  1.00 |    0.00 |   0.6561 |   0.0153 |        - |    10.77 KB |        1.00 |
| BuildInlineDiffModel     | ManySmallChanges     | Small |     13.92 μs |     4.420 μs |   0.242 μs |  1.06 |    0.01 |   0.8240 |   0.0305 |        - |    13.61 KB |        1.26 |
| CreateLineDiffs          | ManySmallChanges     | Small |     14.02 μs |     2.769 μs |   0.152 μs |  1.07 |    0.01 |   0.6561 |   0.0153 |        - |    10.84 KB |        1.01 |
| CreateCharacterDiffs     | ManySmallChanges     | Small |     17.99 μs |     1.444 μs |   0.079 μs |  1.37 |    0.01 |   1.4954 |   0.0916 |        - |     24.7 KB |        2.29 |
| BuildSideBySideDiffModel | ManySmallChanges     | Small |     25.92 μs |     2.840 μs |   0.156 μs |  1.98 |    0.01 |   2.3499 |   0.2136 |        - |    38.86 KB |        3.61 |
| CreateWordDiffs          | ManySmallChanges     | Small |     40.82 μs |     6.619 μs |   0.363 μs |  3.11 |    0.06 |   3.1738 |   0.3662 |        - |    52.72 KB |        4.90 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateCharacterDiffs     | ManySmallChanges     | Large |     88.80 μs |    16.386 μs |   0.898 μs |  0.09 |    0.00 |   5.7373 |   1.3428 |        - |    94.62 KB |        0.16 |
| CreateDiffs_LineChunker  | ManySmallChanges     | Large |    974.73 μs |   184.194 μs |  10.096 μs |  1.00 |    0.00 |  35.1563 |  25.3906 |        - |   600.35 KB |        1.00 |
| CreateLineDiffs          | ManySmallChanges     | Large |  1,034.34 μs |   295.396 μs |  16.192 μs |  1.06 |    0.01 |  35.1563 |  25.3906 |        - |   600.42 KB |        1.00 |
| BuildInlineDiffModel     | ManySmallChanges     | Large |  1,080.94 μs |   239.085 μs |  13.105 μs |  1.11 |    0.00 |  44.9219 |  29.2969 |        - |   761.55 KB |        1.27 |
| BuildSideBySideDiffModel | ManySmallChanges     | Large |  1,939.46 μs |   240.490 μs |  13.182 μs |  1.99 |    0.01 | 123.0469 | 111.3281 |        - |  2038.47 KB |        3.40 |
| CreateWordDiffs          | ManySmallChanges     | Large |  4,079.09 μs |   497.673 μs |  27.279 μs |  4.18 |    0.02 | 398.4375 | 398.4375 | 398.4375 |  3119.17 KB |        5.20 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateDiffs_LineChunker  | FewLargeChanges      | Small |     12.97 μs |     0.893 μs |   0.049 μs |  1.00 |    0.00 |   0.6409 |   0.0153 |        - |    10.62 KB |        1.00 |
| BuildInlineDiffModel     | FewLargeChanges      | Small |     13.90 μs |     1.315 μs |   0.072 μs |  1.07 |    0.00 |   0.8240 |   0.0305 |        - |    13.62 KB |        1.28 |
| CreateLineDiffs          | FewLargeChanges      | Small |     16.83 μs |     3.251 μs |   0.178 μs |  1.30 |    0.01 |   0.6409 |        - |        - |    10.69 KB |        1.01 |
| CreateCharacterDiffs     | FewLargeChanges      | Small |     23.14 μs |     4.491 μs |   0.246 μs |  1.78 |    0.01 |   1.4343 |   0.0916 |        - |    23.47 KB |        2.21 |
| BuildSideBySideDiffModel | FewLargeChanges      | Small |     39.35 μs |     5.839 μs |   0.320 μs |  3.03 |    0.01 |   3.9063 |   0.4272 |        - |    63.86 KB |        6.01 |
| CreateWordDiffs          | FewLargeChanges      | Small |     57.58 μs |     4.386 μs |   0.240 μs |  4.44 |    0.00 |   3.4180 |   0.4272 |        - |    56.72 KB |        5.34 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateCharacterDiffs     | FewLargeChanges      | Large |    192.87 μs |    24.754 μs |   1.357 μs |  0.17 |    0.00 |   5.3711 |   1.2207 |        - |    89.38 KB |        0.15 |
| CreateLineDiffs          | FewLargeChanges      | Large |  1,134.74 μs |   201.686 μs |  11.055 μs |  1.00 |    0.01 |  35.1563 |  23.4375 |        - |   587.34 KB |        1.00 |
| BuildInlineDiffModel     | FewLargeChanges      | Large |  1,135.70 μs |   335.185 μs |  18.373 μs |  1.00 |    0.01 |  44.9219 |  42.9688 |        - |   760.19 KB |        1.29 |
| CreateDiffs_LineChunker  | FewLargeChanges      | Large |  1,136.63 μs |   202.828 μs |  11.118 μs |  1.00 |    0.00 |  35.1563 |  21.4844 |        - |   587.27 KB |        1.00 |
| BuildSideBySideDiffModel | FewLargeChanges      | Large |  2,986.63 μs |   248.855 μs |  13.641 μs |  2.63 |    0.01 | 226.5625 | 203.1250 |        - |  3762.32 KB |        6.41 |
| CreateWordDiffs          | FewLargeChanges      | Large | 46,639.30 μs | 7,007.730 μs | 384.117 μs | 41.03 |    0.27 | 363.6364 | 363.6364 | 363.6364 |  3328.73 KB |        5.67 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateDiffs_LineChunker  | CompletelyDifferent  | Small |     15.31 μs |     1.913 μs |   0.105 μs |  1.00 |    0.00 |   0.7935 |   0.0305 |        - |    13.02 KB |        1.00 |
| CreateLineDiffs          | CompletelyDifferent  | Small |     15.65 μs |     5.080 μs |   0.278 μs |  1.02 |    0.02 |   0.7935 |   0.0305 |        - |    13.09 KB |        1.01 |
| BuildInlineDiffModel     | CompletelyDifferent  | Small |     17.71 μs |     2.514 μs |   0.138 μs |  1.16 |    0.00 |   1.0986 |   0.0305 |        - |    18.11 KB |        1.39 |
| CreateWordDiffs          | CompletelyDifferent  | Small |     48.12 μs |     7.304 μs |   0.400 μs |  3.14 |    0.00 |   3.2959 |   0.4272 |        - |    54.71 KB |        4.20 |
| BuildSideBySideDiffModel | CompletelyDifferent  | Small |     98.67 μs |    33.563 μs |   1.840 μs |  6.45 |    0.12 |  10.8643 |   3.0518 |        - |   177.52 KB |       13.63 |
| CreateCharacterDiffs     | CompletelyDifferent  | Small |    224.24 μs |   109.025 μs |   5.976 μs | 14.65 |    0.49 |   1.4648 |        - |        - |    24.53 KB |        1.88 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateCharacterDiffs     | CompletelyDifferent  | Large |  3,216.54 μs |   344.145 μs |  18.864 μs |  0.40 |    0.00 |   3.9063 |        - |        - |    90.44 KB |        0.13 |
| CreateDiffs_LineChunker  | CompletelyDifferent  | Large |  8,070.43 μs | 1,648.049 μs |  90.335 μs |  1.00 |    0.00 |  46.8750 |  31.2500 |  15.6250 |   693.03 KB |        1.00 |
| BuildInlineDiffModel     | CompletelyDifferent  | Large |  8,223.16 μs | 1,241.614 μs |  68.057 μs |  1.02 |    0.00 |  46.8750 |  31.2500 |  15.6250 |   991.73 KB |        1.43 |
| CreateLineDiffs          | CompletelyDifferent  | Large |  8,276.27 μs | 2,924.963 μs | 160.327 μs |  1.03 |    0.02 |  46.8750 |  31.2500 |  15.6250 |    693.1 KB |        1.00 |
| BuildSideBySideDiffModel | CompletelyDifferent  | Large | 17,702.56 μs | 1,681.268 μs |  92.156 μs |  2.19 |    0.02 | 625.0000 | 500.0000 | 156.2500 | 10543.03 KB |       15.21 |
| CreateWordDiffs          | CompletelyDifferent  | Large | 19,389.63 μs | 1,586.972 μs |  86.987 μs |  2.40 |    0.02 | 343.7500 | 343.7500 | 343.7500 |   3237.9 KB |        4.67 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateLineDiffs          | LongC(...)uence [21] | Small |     13.20 μs |     1.397 μs |   0.077 μs |  0.78 |    0.01 |   0.6409 |   0.0153 |        - |    10.65 KB |        1.01 |
| BuildInlineDiffModel     | LongC(...)uence [21] | Small |     14.69 μs |     2.380 μs |   0.130 μs |  0.87 |    0.01 |   0.8087 |   0.0305 |        - |    13.27 KB |        1.25 |
| CreateCharacterDiffs     | LongC(...)uence [21] | Small |     16.86 μs |     2.796 μs |   0.153 μs |  1.00 |    0.00 |   1.4954 |   0.0916 |        - |    24.54 KB |        2.32 |
| CreateDiffs_LineChunker  | LongC(...)uence [21] | Small |     16.90 μs |     3.875 μs |   0.212 μs |  1.00 |    0.00 |   0.6409 |        - |        - |    10.58 KB |        1.00 |
| BuildSideBySideDiffModel | LongC(...)uence [21] | Small |     20.58 μs |     2.823 μs |   0.155 μs |  1.22 |    0.01 |   1.5564 |   0.0916 |        - |     25.8 KB |        2.44 |
| CreateWordDiffs          | LongC(...)uence [21] | Small |     42.93 μs |     6.288 μs |   0.345 μs |  2.54 |    0.02 |   3.2349 |   0.3662 |        - |    52.93 KB |        5.00 |
|                          |                      |       |              |              |            |       |         |          |          |          |             |             |
| CreateCharacterDiffs     | LongC(...)uence [21] | Large |     64.77 μs |    16.499 μs |   0.904 μs |  0.07 |    0.00 |   5.7373 |   1.3428 |        - |    93.73 KB |        0.16 |
| CreateLineDiffs          | LongC(...)uence [21] | Large |    806.75 μs |   221.576 μs |  12.145 μs |  0.93 |    0.01 |  35.1563 |  26.3672 |        - |   588.32 KB |        1.00 |
| BuildInlineDiffModel     | LongC(...)uence [21] | Large |    866.57 μs |    93.162 μs |   5.107 μs |  1.00 |    0.01 |  44.9219 |  33.2031 |        - |   740.08 KB |        1.26 |
| CreateDiffs_LineChunker  | LongC(...)uence [21] | Large |    870.23 μs |   172.279 μs |   9.443 μs |  1.00 |    0.00 |  35.1563 |  25.3906 |        - |   588.25 KB |        1.00 |
| BuildSideBySideDiffModel | LongC(...)uence [21] | Large |    995.12 μs |   218.820 μs |  11.994 μs |  1.14 |    0.00 |  60.5469 |  29.2969 |        - |   989.58 KB |        1.68 |
| CreateWordDiffs          | LongC(...)uence [21] | Large |  4,967.33 μs |   386.605 μs |  21.191 μs |  5.71 |    0.05 | 398.4375 | 398.4375 | 398.4375 |  3110.18 KB |        5.29 |

## Before/after summary

Positive percentages below indicate improvement. The largest retained gains are in side-by-side builder allocation usage, from pre-sizing output lists and avoiding a final LINQ rescan for the change summary. Some unchanged core/word-diff benchmarks moved in both directions between runs, so those timing-only differences are treated as benchmark noise rather than optimization impact.

| Method | Scenario | Size | Baseline Mean | Final Mean | Time Change | Baseline Allocated | Final Allocated | Allocation Change |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| CreateLineDiffs | Identical | Small | 13.62 μs | 13.70 μs | -0.6% | 10.48 KB | 10.41 KB | 0.7% |
| CreateCharacterDiffs | Identical | Small | 14.39 μs | 14.86 μs | -3.3% | 23.34 KB | 23.32 KB | 0.1% |
| BuildSideBySideDiffModel | Identical | Large | 874.99 μs | 942.19 μs | -7.7% | 881.17 KB | 840.17 KB | 4.7% |
| BuildSideBySideDiffModel | ManySmallChanges | Small | 25.92 μs | 25.33 μs | 2.3% | 38.86 KB | 35.46 KB | 8.7% |
| BuildSideBySideDiffModel | ManySmallChanges | Large | 1,939.46 μs | 1,675.72 μs | 13.6% | 2038.47 KB | 1863.92 KB | 8.6% |
| BuildSideBySideDiffModel | FewLargeChanges | Large | 2,986.63 μs | 2,842.09 μs | 4.8% | 3762.32 KB | 3475.27 KB | 7.6% |
| BuildSideBySideDiffModel | CompletelyDifferent | Large | 17,702.56 μs | 16,892.68 μs | 4.6% | 10543.03 KB | 9998.19 KB | 5.2% |
| BuildSideBySideDiffModel | Long common subsequence | Large | 995.12 μs | 946.13 μs | 4.9% | 989.58 KB | 988.12 KB | 0.1% |
| CreateWordDiffs | ManySmallChanges | Large | 4,079.09 μs | 4,075.55 μs | 0.1% | 3119.17 KB | 3119.17 KB | 0.0% |

## Optimization notes and ROI conclusion

Retained changes:

- `Differ.CreateLineDiffs` and `Differ.CreateCharacterDiffs` now reuse stateless chunker singleton instances, removing one small per-call allocation while preserving the public API and output behavior.
- `SideBySideDiffBuilder` pre-sizes output line lists and tracks whether changes were emitted while building pieces, avoiding a post-build LINQ rescan.
- `DiffPaneModel.HasDifferences` now uses a simple indexed loop instead of LINQ.

Measured but not retained:

- Replacing delimiter `Array.IndexOf` with a `HashSet<char>` increased `CreateWordDiffs` allocation and did not provide consistent timing wins for the default small delimiter set.
- Pre-sizing inline builder output reduced a small amount of allocation but added an extra pass over diff blocks and produced mixed timing results.

Further likely improvements would require higher-risk algorithmic or data-structure changes in the Myers/LCS implementation or chunk representation. Given the mixed timing noise and the clear-but-modest allocation wins from the retained low-risk changes, this pass stops here as the next changes have low expected ROI relative to compatibility and behavior risk.

## Final

Captured after the retained optimization pass in this PR.

```

BenchmarkDotNet v0.13.12, Ubuntu 24.04.4 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 9.0.317
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  Job-EXTCWP : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Runtime=.NET 8.0  IterationCount=3  LaunchCount=1  
WarmupCount=1  

```
| Method                   | Scenario             | Size  | Mean         | Error        | StdDev     | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------------- |--------------------- |------ |-------------:|-------------:|-----------:|------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| CreateLineDiffs          | Identical            | Small |     13.70 μs |     1.077 μs |   0.059 μs |  0.99 |    0.01 |   0.6256 |   0.0153 |        - |   10.41 KB |        1.00 |
| CreateDiffs_LineChunker  | Identical            | Small |     13.79 μs |     2.335 μs |   0.128 μs |  1.00 |    0.00 |   0.6256 |   0.0153 |        - |   10.41 KB |        1.00 |
| BuildInlineDiffModel     | Identical            | Small |     14.63 μs |     0.425 μs |   0.023 μs |  1.06 |    0.01 |   0.7935 |   0.0305 |        - |   12.98 KB |        1.25 |
| CreateCharacterDiffs     | Identical            | Small |     14.86 μs |     4.109 μs |   0.225 μs |  1.08 |    0.01 |   1.4191 |   0.0916 |        - |   23.32 KB |        2.24 |
| BuildSideBySideDiffModel | Identical            | Small |     17.96 μs |     3.441 μs |   0.189 μs |  1.30 |    0.01 |   0.9155 |   0.0305 |        - |   14.96 KB |        1.44 |
| CreateWordDiffs          | Identical            | Small |     39.27 μs |     8.815 μs |   0.483 μs |  2.85 |    0.02 |   3.1738 |   0.3662 |        - |   52.08 KB |        5.00 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateCharacterDiffs     | Identical            | Large |     57.39 μs |     6.506 μs |   0.357 μs |  0.07 |    0.00 |   5.4321 |   1.0376 |        - |   89.23 KB |        0.15 |
| CreateDiffs_LineChunker  | Identical            | Large |    785.11 μs |   115.082 μs |   6.308 μs |  1.00 |    0.00 |  35.1563 |  22.4609 |        - |  582.11 KB |        1.00 |
| BuildInlineDiffModel     | Identical            | Large |    817.88 μs |    56.223 μs |   3.082 μs |  1.04 |    0.01 |  43.9453 |  28.3203 |        - |  731.56 KB |        1.26 |
| CreateLineDiffs          | Identical            | Large |    842.10 μs |   160.964 μs |   8.823 μs |  1.07 |    0.01 |  35.1563 |  22.4609 |        - |  582.11 KB |        1.00 |
| BuildSideBySideDiffModel | Identical            | Large |    942.19 μs |   332.279 μs |  18.213 μs |  1.20 |    0.03 |  50.7813 |  34.1797 |        - |  840.17 KB |        1.44 |
| CreateWordDiffs          | Identical            | Large |  3,024.70 μs |   554.037 μs |  30.369 μs |  3.85 |    0.04 | 371.0938 | 371.0938 | 371.0938 | 3081.63 KB |        5.29 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateLineDiffs          | ManySmallChanges     | Small |     14.06 μs |     2.047 μs |   0.112 μs |  0.98 |    0.01 |   0.6561 |   0.0153 |        - |   10.77 KB |        1.00 |
| CreateDiffs_LineChunker  | ManySmallChanges     | Small |     14.28 μs |     0.892 μs |   0.049 μs |  1.00 |    0.00 |   0.6561 |   0.0153 |        - |   10.77 KB |        1.00 |
| BuildInlineDiffModel     | ManySmallChanges     | Small |     14.96 μs |     2.423 μs |   0.133 μs |  1.05 |    0.01 |   0.8240 |   0.0305 |        - |   13.61 KB |        1.26 |
| CreateCharacterDiffs     | ManySmallChanges     | Small |     18.09 μs |     3.366 μs |   0.185 μs |  1.27 |    0.01 |   1.4954 |   0.0916 |        - |   24.68 KB |        2.29 |
| BuildSideBySideDiffModel | ManySmallChanges     | Small |     25.33 μs |     6.762 μs |   0.371 μs |  1.77 |    0.02 |   2.1667 |   0.1831 |        - |   35.46 KB |        3.29 |
| CreateWordDiffs          | ManySmallChanges     | Small |     41.51 μs |     6.226 μs |   0.341 μs |  2.91 |    0.02 |   3.1738 |   0.3662 |        - |   52.72 KB |        4.90 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateCharacterDiffs     | ManySmallChanges     | Large |     89.87 μs |    25.690 μs |   1.408 μs |  0.08 |    0.00 |   5.7373 |   1.2207 |        - |   94.59 KB |        0.16 |
| BuildInlineDiffModel     | ManySmallChanges     | Large |  1,077.34 μs |   269.700 μs |  14.783 μs |  0.97 |    0.05 |  44.9219 |  29.2969 |        - |  761.55 KB |        1.27 |
| CreateDiffs_LineChunker  | ManySmallChanges     | Large |  1,109.17 μs |   977.425 μs |  53.576 μs |  1.00 |    0.00 |  35.1563 |  25.3906 |        - |  600.35 KB |        1.00 |
| CreateLineDiffs          | ManySmallChanges     | Large |  1,228.58 μs |   197.915 μs |  10.848 μs |  1.11 |    0.06 |  35.1563 |  25.3906 |        - |  600.35 KB |        1.00 |
| BuildSideBySideDiffModel | ManySmallChanges     | Large |  1,675.72 μs |   620.007 μs |  33.985 μs |  1.51 |    0.09 | 113.2813 |  93.7500 |        - | 1863.92 KB |        3.10 |
| CreateWordDiffs          | ManySmallChanges     | Large |  4,075.55 μs |   298.629 μs |  16.369 μs |  3.68 |    0.17 | 398.4375 | 398.4375 | 398.4375 | 3119.17 KB |        5.20 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateLineDiffs          | FewLargeChanges      | Small |     13.07 μs |     1.354 μs |   0.074 μs |  0.92 |    0.01 |   0.6409 |   0.0153 |        - |   10.62 KB |        1.00 |
| CreateDiffs_LineChunker  | FewLargeChanges      | Small |     14.14 μs |     2.017 μs |   0.111 μs |  1.00 |    0.00 |   0.6409 |   0.0153 |        - |   10.62 KB |        1.00 |
| BuildInlineDiffModel     | FewLargeChanges      | Small |     14.77 μs |     1.320 μs |   0.072 μs |  1.05 |    0.01 |   0.8240 |   0.0305 |        - |   13.62 KB |        1.28 |
| CreateCharacterDiffs     | FewLargeChanges      | Small |     23.57 μs |     6.211 μs |   0.340 μs |  1.67 |    0.04 |   1.4343 |   0.0916 |        - |   23.45 KB |        2.21 |
| BuildSideBySideDiffModel | FewLargeChanges      | Small |     38.36 μs |    10.972 μs |   0.601 μs |  2.71 |    0.06 |   3.6011 |   0.4272 |        - |   59.03 KB |        5.56 |
| CreateWordDiffs          | FewLargeChanges      | Small |     57.75 μs |     6.185 μs |   0.339 μs |  4.09 |    0.05 |   3.4180 |   0.4272 |        - |   56.72 KB |        5.34 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateCharacterDiffs     | FewLargeChanges      | Large |    195.00 μs |    27.998 μs |   1.535 μs |  0.17 |    0.00 |   5.3711 |   1.2207 |        - |   89.35 KB |        0.15 |
| CreateDiffs_LineChunker  | FewLargeChanges      | Large |  1,132.69 μs |   247.261 μs |  13.553 μs |  1.00 |    0.00 |  35.1563 |  21.4844 |        - |  587.27 KB |        1.00 |
| BuildInlineDiffModel     | FewLargeChanges      | Large |  1,145.58 μs |   274.827 μs |  15.064 μs |  1.01 |    0.00 |  44.9219 |  42.9688 |        - |  760.21 KB |        1.29 |
| CreateLineDiffs          | FewLargeChanges      | Large |  1,168.24 μs |   885.656 μs |  48.546 μs |  1.03 |    0.05 |  35.1563 |  21.4844 |        - |  587.27 KB |        1.00 |
| BuildSideBySideDiffModel | FewLargeChanges      | Large |  2,842.09 μs | 1,148.873 μs |  62.974 μs |  2.51 |    0.03 | 210.9375 | 187.5000 |        - | 3475.27 KB |        5.92 |
| CreateWordDiffs          | FewLargeChanges      | Large | 46,409.79 μs | 3,809.567 μs | 208.815 μs | 40.98 |    0.63 | 363.6364 | 363.6364 | 363.6364 | 3328.73 KB |        5.67 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateDiffs_LineChunker  | CompletelyDifferent  | Small |     16.35 μs |     2.461 μs |   0.135 μs |  1.00 |    0.00 |   0.7935 |   0.0305 |        - |   13.02 KB |        1.00 |
| CreateLineDiffs          | CompletelyDifferent  | Small |     16.50 μs |     4.997 μs |   0.274 μs |  1.01 |    0.01 |   0.7935 |   0.0305 |        - |   13.02 KB |        1.00 |
| BuildInlineDiffModel     | CompletelyDifferent  | Small |     17.36 μs |     3.310 μs |   0.181 μs |  1.06 |    0.00 |   1.0986 |   0.0305 |        - |   18.11 KB |        1.39 |
| CreateWordDiffs          | CompletelyDifferent  | Small |     48.35 μs |    14.064 μs |   0.771 μs |  2.96 |    0.02 |   3.2959 |   0.4272 |        - |   54.71 KB |        4.20 |
| BuildSideBySideDiffModel | CompletelyDifferent  | Small |     92.27 μs |     8.958 μs |   0.491 μs |  5.64 |    0.02 |  10.2539 |   2.9297 |        - |  168.39 KB |       12.93 |
| CreateCharacterDiffs     | CompletelyDifferent  | Small |    226.16 μs |    30.315 μs |   1.662 μs | 13.84 |    0.02 |   1.4648 |        - |        - |   24.51 KB |        1.88 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateCharacterDiffs     | CompletelyDifferent  | Large |  3,372.52 μs |   338.465 μs |  18.552 μs |  0.40 |    0.00 |   3.9063 |        - |        - |   90.42 KB |        0.13 |
| CreateLineDiffs          | CompletelyDifferent  | Large |  8,161.70 μs |   516.252 μs |  28.298 μs |  0.97 |    0.00 |  46.8750 |  31.2500 |  15.6250 |  693.03 KB |        1.00 |
| CreateDiffs_LineChunker  | CompletelyDifferent  | Large |  8,436.76 μs |   768.326 μs |  42.115 μs |  1.00 |    0.00 |  46.8750 |  31.2500 |  15.6250 |  693.03 KB |        1.00 |
| BuildInlineDiffModel     | CompletelyDifferent  | Large |  8,531.53 μs | 1,318.007 μs |  72.244 μs |  1.01 |    0.00 |  46.8750 |  31.2500 |  15.6250 |  991.73 KB |        1.43 |
| BuildSideBySideDiffModel | CompletelyDifferent  | Large | 16,892.68 μs | 3,345.925 μs | 183.401 μs |  2.00 |    0.01 | 593.7500 | 500.0000 | 187.5000 | 9998.19 KB |       14.43 |
| CreateWordDiffs          | CompletelyDifferent  | Large | 19,645.50 μs | 1,221.744 μs |  66.968 μs |  2.33 |    0.00 | 343.7500 | 343.7500 | 343.7500 |  3237.9 KB |        4.67 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateLineDiffs          | LongC(...)uence [21] | Small |     13.80 μs |     2.279 μs |   0.125 μs |  0.99 |    0.04 |   0.6409 |   0.0153 |        - |   10.58 KB |        1.00 |
| CreateDiffs_LineChunker  | LongC(...)uence [21] | Small |     14.02 μs |     7.660 μs |   0.420 μs |  1.00 |    0.00 |   0.6409 |   0.0153 |        - |   10.58 KB |        1.00 |
| BuildInlineDiffModel     | LongC(...)uence [21] | Small |     14.50 μs |     0.425 μs |   0.023 μs |  1.04 |    0.03 |   0.8087 |   0.0305 |        - |   13.27 KB |        1.25 |
| CreateCharacterDiffs     | LongC(...)uence [21] | Small |     15.68 μs |     8.267 μs |   0.453 μs |  1.12 |    0.01 |   1.4954 |   0.0916 |        - |   24.52 KB |        2.32 |
| BuildSideBySideDiffModel | LongC(...)uence [21] | Small |     19.75 μs |     2.864 μs |   0.157 μs |  1.41 |    0.05 |   1.4648 |   0.0916 |        - |   24.29 KB |        2.30 |
| CreateWordDiffs          | LongC(...)uence [21] | Small |     41.38 μs |     2.799 μs |   0.153 μs |  2.95 |    0.08 |   3.2349 |   0.3662 |        - |   52.93 KB |        5.00 |
|                          |                      |       |              |              |            |       |         |          |          |          |            |             |
| CreateCharacterDiffs     | LongC(...)uence [21] | Large |     59.23 μs |     5.010 μs |   0.275 μs |  0.07 |    0.00 |   5.7373 |   1.4038 |        - |   93.71 KB |        0.16 |
| CreateDiffs_LineChunker  | LongC(...)uence [21] | Large |    842.18 μs |   131.067 μs |   7.184 μs |  1.00 |    0.00 |  35.1563 |  25.3906 |        - |  588.25 KB |        1.00 |
| CreateLineDiffs          | LongC(...)uence [21] | Large |    844.96 μs |   183.045 μs |  10.033 μs |  1.00 |    0.01 |  35.1563 |  25.3906 |        - |  588.25 KB |        1.00 |
| BuildInlineDiffModel     | LongC(...)uence [21] | Large |    909.15 μs |   532.317 μs |  29.178 μs |  1.08 |    0.04 |  44.9219 |  33.2031 |        - |  740.08 KB |        1.26 |
| BuildSideBySideDiffModel | LongC(...)uence [21] | Large |    946.13 μs |   141.596 μs |   7.761 μs |  1.12 |    0.01 |  59.5703 |  43.9453 |        - |  988.12 KB |        1.68 |
| CreateWordDiffs          | LongC(...)uence [21] | Large |  4,678.57 μs | 1,007.041 μs |  55.199 μs |  5.56 |    0.02 | 398.4375 | 398.4375 | 398.4375 | 3110.18 KB |        5.29 |
