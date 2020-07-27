using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace PruningRadixTrie.Benchmark
{
    public class Result
    {
        public long Frequency;
        public string Match;
    }
    public class TestCase
    {
        public string Term;
        public List<Result> Results;
    }
    class Program
    {
        public static void Benchmark()
        {
            Console.WriteLine("Load dictionary & create trie ...");
            PruningRadixTrie pruningRadixTrie = new PruningRadixTrie();
            if (!File.Exists("terms.txt"))
            {
                ZipFile.ExtractToDirectory("terms.zip", ".");
            }
            pruningRadixTrie.ReadTermsFromFile("terms.txt");


            Console.WriteLine("Terms loaded. Enter # of terms to return in result set and press Enter.");
            var resultCount = int.Parse(Console.ReadLine());

            var query = "";
            var queryLog = new List<TestCase>();
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Top {resultCount} Terms");
                if (query.Length > 0)
                {
                    var results = pruningRadixTrie.GetTopkTermsForPrefix(query, resultCount, out long termFrequencyCountPrefix, true);
                    foreach ((string term, long termFrequencyCount) in results) Console.WriteLine(term + " " + termFrequencyCount.ToString("N0"));
                    // Don't add additional items to query log if we've already added them.
                    if (!queryLog.Any(x => x.Term == query))
                    {
                        queryLog.Add(new TestCase() { Results = results.Select(x => new Result() { Frequency = x.termFrequencyCount, Match = x.term }).ToList(), Term = query });
                        File.WriteAllText("queryLog.json", JsonConvert.SerializeObject(queryLog));
                    }
                }
                else
                {
                    Console.WriteLine("Enter more than 1 character to start querying.");
                }
                Console.Write($"> {query}");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Delete)
                {
                    query = query.Substring(0, Math.Max(0, query.Length - 1));
                }
                else
                {
                    query += key.KeyChar;
                }

            }
            int rounds = 1000;
            string queryString = "microsoft";
            for (int i = 0; i < queryString.Length; i++)
            {
                //benchmark Ordinary Radix Trie
                Stopwatch sw = Stopwatch.StartNew();
                for (int loop = 0; loop < rounds; loop++)
                {
                    var results=pruningRadixTrie.GetTopkTermsForPrefix(queryString.Substring(0, i + 1), 10,out long termFrequencyCountPrefix, false);
                    //foreach ((string term, long termFrequencyCount) in results) Console.WriteLine(term + " " + termFrequencyCount.ToString("N0"));
                }
                sw.Stop();
                long time1 = sw.ElapsedMilliseconds;
                Console.WriteLine("ordinary search " + queryString.Substring(0, i + 1) + " in " + ((double)time1 / (double)rounds).ToString("N6") + " ms");
                

                //benchmark Pruning Radix Trie
                sw = Stopwatch.StartNew();
                for (int loop = 0; loop < rounds; loop++)
                {
                    var results = pruningRadixTrie.GetTopkTermsForPrefix(queryString.Substring(0, i + 1), 10, out long termFrequencyCountPrefix, true);
                    //foreach ((string term,long termFrequencyCount) in results) Console.WriteLine(term+" "+termFrequencyCount.ToString("N0"));
                }
                sw.Stop();
                long time2 = sw.ElapsedMilliseconds;
                Console.WriteLine("pruning search " + queryString.Substring(0, i + 1) + " in " + ((double)time2 / (double)rounds).ToString("N6") + " ms");
                

                Console.WriteLine(((double)time1 / (double)time2).ToString("N2") + " x faster");
            }

            Console.WriteLine("press key to exit.");
            Console.ReadKey();
        }


        static void Main(string[] args)
        {
            Benchmark();      
        }
    }
}
