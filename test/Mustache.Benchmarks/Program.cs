using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mustache.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary1 = BenchmarkRunner.Run<FormatCompilerBenchmarks>();
        }
    }
}
