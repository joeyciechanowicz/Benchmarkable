using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Benchmarkable
{
    /// <summary>
    /// Provides benchmarks of functions
    /// </summary>
    public class Benchmark
    {
        private List<(Action action, string label)> actions = new List<(Action, string)>();
        private BenchmarkSettings settings;

        /// <summary>
        /// The default settings used to configure a benchmark. 
        /// </summary>
        public static BenchmarkSettings Settings { get; } = new BenchmarkSettings();

        /// <summary>
        /// Create a new benchmark
        /// </summary>
        public Benchmark()
        {
            settings = Settings;
        }

        /// <summary>
        /// Create a new benchmark with the given settings
        /// </summary>
        /// <param name="settings"></param>
        public Benchmark(BenchmarkSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Run the configured benchmark and return the results
        /// </summary>
        /// <returns>Results of the benchmark</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when no methods have been added to benchmark</exception>
        public BenchmarkResults Run()
        {
            if (actions.Count == 0)
            {
                throw new ArgumentOutOfRangeException("Can not run a benchmark when no methods to benchmark have been added. Call Add(Action) first");
            }

            var results = actions.Select(x => RunAction(x.action, x.label));
            return new BenchmarkResults(results.ToArray());
        }

        /// <summary>
        /// Benchmark just one method
        /// </summary>
        /// <example>Benchmark.Just.This(()=> {}, "my benchmark")</example>
        public static JustClause Just { get; } = new JustClause();

        /// <summary>
        /// Benchmark two methods against each other.
        /// </summary>
        /// <param name="action">First method</param>
        /// <param name="label">A label for this method</param>
        /// <example>
        /// Benchmark.This(method).Against(otherMethod)
        /// </example>
        /// <returns></returns>
        public static ThisClause This(Action action, string label = null)
        {
            var benchmark = new Benchmark();
            benchmark.Add(action, label);
            return new ThisClause(benchmark);
        }

        /// <summary>
        /// Benchmark a number of methods against each other
        /// </summary>
        /// <param name="actions">An array of methods to benchmark. Each method requires a label</param>
        /// <example>
        /// Benchmark.These(new (Action, string)[]
        ///    {
        ///        (() => { }, "my test 1" ),
        ///        (() => { }, "my test 2" ),
        ///        (() => { }, "my test 3" ),
        ///    })
        /// </example>
        /// <returns></returns>
        public static BenchmarkResults These((Action, string)[] actions)
        {
            var benchmark = new Benchmark();
            foreach (var action in actions)
            {
                benchmark.Add(action.Item1, action.Item2);
            }
            return benchmark.Run();
        }

        /// <summary>
        /// Add an action to benchmark
        /// </summary>
        /// <example>Add(() => Math.Sqrt(1.23), "Sqare root benchmark")</example>
        /// <param name="action">The method to benchmark</param>
        /// <param name="label">A name for this benchmark</param>
        public void Add(Action action, string label = null)
        {
            actions.Add(
                (action,
                label ?? $"Test {actions.Count + 1}"));
        }

        private Result RunAction(Action action, string label)
        {
            // Inspiration taken fro
            // http://monsur.hossa.in/2012/12/11/benchmarkjs.html
            // http://ejohn.org/blog/javascript-benchmark-quality/
            // http://ejohn.org/apps/measure/

            var (batchSize, batchTime) = CalculateBatchSize(action);

            double error = double.MaxValue;
            var runTicks = new List<long>();
            var runs = new List<Run>();
            var start = DateTime.Now;

            // Always run it 2 times at least so we can calculate the standard deviation
            while ((error > settings.MinimumErrorToAccept || runTicks.Count <= 2))
            {
                // Give the test as good a chance as possible of avoiding garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < batchSize; i++)
                {
                    action();
                }
                sw.Stop();

                runTicks.Add(sw.ElapsedTicks);
                var runStatistics = CalculateRunStatistics(runTicks, batchSize);
                error = runStatistics.Error;
                
                if ((DateTime.Now - start).TotalMilliseconds > settings.MaxTime)
                {
                    runStatistics.ExceededMaxTime = true;
                    runs.Add(runStatistics);
                    break;
                }

                runs.Add(runStatistics);
                Write($"Error: {error}\tS.D: {runStatistics.StandardDeviation}");
            }

            Write($"Time ran for: {(DateTime.Now - start).TotalMilliseconds}");

            var result = new Result()
            {
                Label = label,
                BatchSize = batchSize,
                BatchTime = batchTime,
                Runs = runs
            };
            return result;
        }

        /// <summary>
        /// Calculate statistics for the current series of batch timings
        /// </summary>
        /// <param name="values"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        private Run CalculateRunStatistics(List<long> values, int batchSize)
        {
            // Only use the last 10 values
            var valuesToUse = values.Skip(Math.Max(0, values.Count() - settings.BatchesToWorkAcross)).ToArray();
            var ticks = values.Last();

            // TODO: Not shitty implementation, use a rolling average and standard deviation
            double mean = valuesToUse.Average();
            double variance = valuesToUse.Average(v => Math.Pow(v - mean, 2));
            double deviation = Math.Sqrt(variance);

            double msSpentOnRun = ticks / (double)Stopwatch.Frequency * 1000d;
            double upgradeFactor = 1000 / msSpentOnRun;
            double operationsPerSecond = batchSize * upgradeFactor;

            int currTDistributionValue = Math.Min(settings.BatchesToWorkAcross, values.Count());
            double standardErrorsMean = deviation / Math.Sqrt(valuesToUse.Count()) * TDistribution.Values[currTDistributionValue];
            double error = Math.Max((standardErrorsMean / mean) * 100, 0);

            return new Run()
            {
                MeanTicks = mean,
                StandardDeviation = deviation,
                Variance = variance,
                StandardErrorsMean = standardErrorsMean,
                Error = error,
                OperationsPerSecond = operationsPerSecond,
                Ticks = ticks
            };
        }

        /// <summary>
        /// Calculates how many times we should run our action per batch
        /// </summary>
        private (int batchSize, int batchTime) CalculateBatchSize(Action action)
        {
            int count = 0;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < settings.InitialBatchTime)
            {
                action();
                count++;
            }

            // TODO: Calculate a good batch size based on how long we're taking to run
            // So slow running actions don't need to be run many times, but fast might need 
            // running a lot for a shorter period of time. For now just give back whatevers in the settings
            // Potential approach could be to run for BatchSizeTime and if the number is below a threshold we give back some
            // other number
            return (count, settings.InitialBatchTime);
        }

        private void Write(string message)
        {
            if (settings.Verbose)
            {
                Console.WriteLine(message);
            }
        }
    }


}

