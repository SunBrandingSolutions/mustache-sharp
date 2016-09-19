using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mustache.Benchmarks
{
    /// <summary>
    /// Benchmarks for formatting mustache content.
    /// </summary>
    /// <remarks>
    /// Tests taken from http://mustache.github.io/mustache.5.html.
    /// </remarks>
    [Config("columns=AllStatistics")]
    public class FormatCompilerBenchmarks
    {
        private const string SmallTemplate = @"Hello {{name}}
You have just won {{value:C}} dollars!
{{#if in_ca}}
Well, {{taxed_value:C}} dollars, after taxes.
{{/if}}";

        private static readonly object SmallTemplateData = new
        {
            name = "Chris",
            value = 10000M,
            taxed_value = 10000M - (10000M * 0.4M),
            in_ca = true
        };

        private FormatCompiler _compiler = new FormatCompiler();

        [Benchmark]
        public void CompileSmallTemplate()
        {
            var gen = _compiler.Compile(SmallTemplate);
        }

        [Benchmark]
        public void RenderSmallTemplate()
        {
            var gen = _compiler.Compile(SmallTemplate);
            var output = gen.Render(SmallTemplateData);
        }
    }
}
