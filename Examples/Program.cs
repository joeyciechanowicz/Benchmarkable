using System;
using Benchmarkable;
using System.Text.RegularExpressions;

namespace Examples
{
    class MainClass
	{
		public static void Main (string[] args)
		{
            Benchmark.Settings.Verbose = true;

            var haystack = "abcdef";
            Benchmark.This(() => haystack.Contains("ef"), "string.Contains")
                .Against(() => new Regex("ef").IsMatch(haystack), "string.IndexOf")
                .Print();

            // Single method
            Benchmark.Just.This(() => { })
                .Print();

            // Two methods
            Benchmark.This(() => { })
                .Against(() => { })
                .Print();

            // Multiple methods
            Benchmark.These(new(Action, string)[]{
                (() => Math.Sin(1.23), "Sin1"),
                (() => Math.Sin(1.23), "Sin2"),
                (() => Math.Sin(1.23), "Sin3"),
                (() => Math.Sin(1.23), "Sin4"),
                (() => Math.Sin(1.23), "Sin5"),
            }).Print();

            // Explicit usage
            var benchmark = new Benchmark();
            benchmark.Add(() => { }, "Label");
            benchmark.Add(() => { }, "Label 2");
            var results = benchmark.Run();
            results.Print();

            Console.Read();
		}
	}
}
