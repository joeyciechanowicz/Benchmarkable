Benchmarkable
===========

Simple easy .NET benchmarking for little bits of code that uses probability to give oyu accurate results regardless of how fast your CPU is. When you benchmark a method it will do a sample run to 
see how many iterations per batch it should run of your method. Then it will run batch after batch until the error percentage is below 1% (the default, this can be changed though). 

The idea is that even if your CPU is insanely fast or really slow you will still get a statistically accurate result. 

## Install
Run the following command in the Package Manager Console (NuGet).
```bash
PM> Install-Package Benchmarkable
```
Or clone and include Benchmarkable.csproj directly

## Use
Lets say you wanted to see if string.Contains was faster or slower than using a regular expression
```csharp
var haystack = "abcdef";
Benchmark.This(() => haystack.Contains("ef"), "string.Contains")
    .Against(() => new Regex("ef").IsMatch(haystack), "Regex.IsMatch")
    .Print();
```
```
Label            |Runs      |Ops/Sec                  |Times slower
-----------------+----------+-------------------------+---------------
string.Contains  |27        |13,773,246 +/-3.460%     |1.000x
Regex.IsMatch    |10        |459,500 +/-9.674%        |29.974x
```

### Single method
```csharp
Benchmark.Just.This(() => Math.Sin(1.23))
	.Print();
```

### Two methods
```csharp
Benchmark.This(() => { })
    .Against(() => { }, "With a label")
    .Print();
```

### Multiple methods
```csharp
Benchmark.These(new(Action, string)[]{
    (() => Math.Sin(1.23), "Sin1"),
    (() => Math.Sin(1.23), "Sin2"),
    (() => Math.Sin(1.23), "Sin3"),
    (() => Math.Sin(1.23), "Sin4"),
    (() => Math.Sin(1.23), "Sin5"),
}).Print();
```
_Note: You have to provide labels for each method you want to benchmark when using this syntax_

### Explicit declaration
```csharp
var benchmark = new Benchmark();
    benchmark.Add(() => { }, "Label");
    benchmark.Add(() => { }, "Label 2");
    var results = benchmark.Run();
    results.Print();
```

### Benchmark.Settings
The settings must be changed *before* creating a benchmark.

| Value                                     | Default | Description                                                                                             |
|-------------------------------------------|---------|---------------------------------------------------------------------------------------------------------|
| `Benchmark.Settings.InitialBatchTime`     | 500ms   | The time to run a benchmark method for initially to calculate the batch size                            |
| `Benchmark.Settings.MinimumErrorToAccept` | 1.0     | Error % over the last 10 (default) batches to accept as a valid benchmark result                        |
| `Benchmark.Settings.BatchesToWorkAcross`  | 10      | Number of batches to calculate statistics across (and thus error%)                                      |
| `Benchmark.Settings.MaxTime`             | 5000ms  | Maximum time to run a single method for before terminating, this is if the error % has not yet been met |
| `Benchmark.Settings.Verbose`              | false   | Print outinformation as a benchmark is run.                                                             |


### Contributing
Any and all help is greatly appreciated. Especially around the statistical elements.

# TODO
* Printing of results, needs to be cleaned up made more versatile. Maybe some lightweight templates
* Automatically calculate the batch size to one that will lower the error % as fast as possible.
