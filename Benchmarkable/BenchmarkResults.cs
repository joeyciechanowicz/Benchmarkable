using System;
using System.Collections.Generic;
using System.Linq;

namespace Benchmarkable
{
    public class BenchmarkResults
    {
        private Result[] results;

        internal BenchmarkResults(Result[] results)
        {
            this.results = results;
        }

        public Result this[int i]
        {
            get
            {
                return results[i];
            }
        }
        
        public int Length { get { return results.Length; } }

        // TODO: Clean this up, it's gross
        public void Print()
        {
            var fastest = results.OrderBy(x => x.OperationsPerSecond).Last();

            var output = new List<string[]>();
            var longestLabel = 0;
            var fastestRow = 0;

            foreach (var result in results)
            {
                var amountSlower = fastest.OperationsPerSecond / result.OperationsPerSecond;

                output.Add(new string[] {
                    result.Label,
                    result.Runs.Count().ToString(),
                    SencibleDouble(result.OperationsPerSecond) + " +/-" + SencibleDouble(result.Error) + "%",
                    SencibleDouble(amountSlower) + "x"
                });

                longestLabel = Math.Max(longestLabel, result.Label.Length);
                fastestRow = amountSlower == 0.0d ? output.Count() - 1 : fastestRow;
            }

            PrintOutput(output, longestLabel, fastestRow);
        }

        private void PrintOutput(List<string[]> output, int longestLabel, int fastestRow)
        {
            var prevColor = Console.ForegroundColor;

            //output.Add(String.Format("{0,-10}|{1,-10}|{2,-25}|{3,-15}", "Label", "Runs", "Ops/Sec", "% Slower"));
            //output.Add(String.Format("{0,-10}+{0,-10}+{1,-25}+{2,-15}", new String('-', 10), new String('-', 25), new String('-', 15)));
            var formatString = $"{{0,-{longestLabel+2}}}|{{1,-10}}|{{2,-25}}|{{3,-15}}";

            Console.WriteLine(String.Format(formatString, "Label", "Runs", "Ops/Sec", "Times slower"));
            Console.WriteLine(String.Format(formatString.Replace('|', '+'), new String('-', longestLabel + 2), new String('-', 10), new String('-', 25), new String('-', 15)));

            // This is purpoesfully split as at some point it would be good to offer a more comprehencive output
            for (var i = 0; i < output.Count(); i++)
            {
                if (i == fastestRow)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(String.Format(formatString, output[i]));
                Console.ForegroundColor = prevColor;
            }
        }

        private string SencibleDouble(double input)
        {
            if (input > 1000)
            {
                return String.Format("{0:n0}", input);
            }
            return String.Format("{0:n3}", input);
        }
    }
}