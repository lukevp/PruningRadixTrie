using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace PruningRadixTrie.Benchmark
{
    // Add this attribute to compare runtime of .NET472 and .NET Core 3.1 (roughly 20% faster on .NET Core)
    // [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [RPlotExporter]
    [HtmlExporter]
    [CsvMeasurementsExporter]
    [MarkdownExporterAttribute.GitHub]
    public class RegularRadixTrieVsPrefixTrie
    {
        private PruningRadixTrie termsTestTrie;
        private string queryString = "microsoft";

        // To run all possible variants of microsoft, use the below.
        [Params(1, 2, 3, 4, 5, 6, 7, 8, 9)]
        // To do a quick comparison / graph test, use the below.
        // [Params(1, 9)]
        public int SubstringLength;

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine("Load dictionary & create trie ...");
            termsTestTrie = new PruningRadixTrie();
            if (!File.Exists("terms.txt"))
            {
                ZipFile.ExtractToDirectory("terms.zip", ".");
            }
            termsTestTrie.ReadTermsFromFile("terms.txt");
        }

        [Benchmark(Baseline = true)]
        public List<(string, long)> RegularRadixTrie() => termsTestTrie.GetTopkTermsForPrefix(queryString.Substring(0, SubstringLength), 10, out long termFrequencyCountPrefix, false);


        [Benchmark]
        public List<(string, long)> PrefixRadixTrie() => termsTestTrie.GetTopkTermsForPrefix(queryString.Substring(0, SubstringLength), 10, out long termFrequencyCountPrefix, true);
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Use this for debugging benchmarks.
            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            BenchmarkRunner.Run<RegularRadixTrieVsPrefixTrie>();
        }
    }
}
