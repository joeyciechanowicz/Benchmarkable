using System;

namespace Benchmarkable
{
    /// <summary>
    /// Settings that control the running of a benchmark.
    /// </summary>
    public class BenchmarkSettings
    {
        /// <summary>
        /// The minimum amount of time a batch should take to run
        /// </summary>
        public int InitialBatchTime { get; set; } = 500;
        
        /// <summary>
        /// The acceptable MSE of the last 10 (default) batches.
        /// </summary>
        public double MinimumErrorToAccept { get; set; } = 1.0d;

        /// <summary>
        /// The number of batches to keep and to calculate statistcs for
        /// </summary>
        private int batchesToWorkAcross = 10;
        public int BatchesToWorkAcross
        {
            get
            {
                return batchesToWorkAcross;
            }
            set
            {
                if (value > TDistribution.Values.Length)
                {
                    throw new ArgumentOutOfRangeException($"Maximum value of batches is {TDistribution.Values.Length} as that is the number of t-distrubtion values stored");
                }
                batchesToWorkAcross = value;
            }
        }

        /// <summary>
        /// Maximum amount of time to benchmark for if we haven't got to a standard deviation within our acceptable amount
        /// </summary>
        public int MaxTime { get; set; } = 5000;

        /// <summary>
        /// Write out information as the benchmark runs
        /// </summary>
        public bool Verbose { get; set; } = false;
    }
}