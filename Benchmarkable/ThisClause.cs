using System;

namespace Benchmarkable
{
    public class ThisClause
    {
        private Benchmark benchmark;

        internal ThisClause(Benchmark benchmark)
        {
            this.benchmark = benchmark;
        }

        /// <summary>
        /// Method to benchmark against
        /// </summary>
        /// <param name="action">Second method</param>
        /// <param name="label">Label for the method</param>
        /// <returns>Results of the benchmark</returns>
        public BenchmarkResults Against(Action action, string label = null)
        {
            benchmark.Add(action, label);
            return benchmark.Run();
        }
    }
}