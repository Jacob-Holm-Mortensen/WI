using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class Ranker
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "htmls\\");
        Indexer i;
        Crawler c;
        Program p = new Program();
        Robot r = new Robot();

        public Ranker(Crawler _c, Indexer _i)
        {
            c = _c;
            i = _i;
        }

        public List<KeyValuePair<int, double>> GetPagesWithWords(string words, Dictionary<string, Dictionary<int, double>> index)
        {
            List<string> split = i.RemoveStopWords(words.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList());
            Dictionary<string, Dictionary<int, double>> output, tf, tfidf = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, double> idf = new Dictionary<string, double>();
            List<KeyValuePair<int, double>> pages = new List<KeyValuePair<int, double>>();
            Dictionary<int, List<double>> vectors = new Dictionary<int, List<double>>();

            tf = tfCalc(index);
            idf = idfCalc(index);
            tfidf = tfidfCalc(tf, idf);
            // implement vector compare

            // Use tfidf comparison
            output = tfidf.Where(x => split.Any(z => z == x.Key)).ToDictionary(x => x.Key, x => x.Value);

            // Make vectors
            //vectors = CreateVectors(output, tfidf);

            // Make vector comparison

            if (output.Count > 0)
            {
                pages = output[output.Keys.First()].ToList();
                foreach (var key in output.Keys.ToList())
                {
                    pages = pages.Where(x => output[key].ContainsKey(x.Key)).ToList();
                }
                pages.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            }
            return pages;
        }

        public Dictionary<int, List<double>> CreateVectors(Dictionary<string, Dictionary<int, double>> search, Dictionary<string, Dictionary<int, double>> tfidf)
        {
            Dictionary<int, List<double>> vectors = new Dictionary<int, List<double>>();
            HashSet<int> pages = new HashSet<int>();
            foreach (var dic in search.Values.ToList())
            {
                foreach (var page in dic.Keys.ToList())
                {
                    pages.Add(page);
                }
            }
            vectors.Add(-1, new List<double>());
            foreach (var term in search) vectors[-1].Add(1);
            foreach (var page in pages)
            {
                vectors.Add(page, new List<double>());
                foreach (var term in search)
                {
                    if (term.Value.ContainsKey(page))
                    {
                        vectors[page].Add(tfidf[term.Key][page]);
                    }
                    else vectors[page].Add(0);
                }
            }
            return vectors;
        }

        public void PrintSearchResult(List<KeyValuePair<int, double>> pages, string words)
        {
            if (pages.Count > 0)
            {
                Console.WriteLine("\n10 most relavant pages");
                foreach (var indexPage in pages)
                {
                    if (pages.IndexOf(indexPage) < 10) Console.WriteLine(c.getUrlFromMapping(i.pageNames[indexPage.Key]));
                    else break;
                }
                Console.WriteLine("Of the crawled pages " + pages.Count + " contained the words (-stopwords): " + words + "\n");
            }
            else Console.WriteLine("No match\n");
        }

        public Dictionary<string, Dictionary<int, double>> tfCalc(Dictionary<string, Dictionary<int, double>> input)
        {
            Dictionary<string, Dictionary<int, double>> output = input;
            foreach (var dic in output.Values.ToList())
            {
                foreach (var page in dic.Keys.ToList())
                {
                    if (dic[page] > 0) dic[page] = 1 + Math.Log10(dic[page]);
                    else dic[page] = 0;

                }
            }
            return output;
        }

        public Dictionary<string, double> idfCalc(Dictionary<string, Dictionary<int, double>> input)
        {
            Dictionary<string, double> output = new Dictionary<string, double>();
            foreach (var key in input.Keys.ToList())
            {
                output.Add(key, Math.Log10(1000 / input[key].Count()));
            }
            return output;
        }

        public Dictionary<string, Dictionary<int, double>> tfidfCalc(Dictionary<string, Dictionary<int, double>> tf, Dictionary<string, double> idf)
        {
            Dictionary<string, Dictionary<int, double>> output = tf;
            foreach (var key in output.Keys.ToList())
            {
                foreach (var page in output[key].Keys.ToList())
                {
                    output[key][page] = output[key][page] * idf[key];
                }
            }
            return output;
        }

        public List<List<double>> MakePageRankMatrix()
        {
            List<List<double>> matrix = new List<List<double>>();
            List<string> urls = new List<string>();
            foreach (var fileName in Directory.GetFiles(baseDir)) if (fileName != baseDir + "mapping") urls.Add(c.getUrlFromMapping(fileName));

            // Initialize matrix
            for (int k = 0; k < urls.Count; k++)
            {
                matrix.Add(new List<double>());
                for (int j = 0; j < urls.Count; j++)
                {
                    matrix[k].Add(0);
                }
                i.printOnLine("Initialised pages: " + (k+1));
            }

            // Fill matrix
            Console.WriteLine();
            foreach (var url in urls)
            {
                if (urls.IndexOf(url) < 2)
                {
                    List<string> urlsPagePointTo = GetUrlsForMatrixValues(url, urls).ToList();
                    foreach (var innerUrl in urlsPagePointTo)
                    {
                        matrix[urls.IndexOf(url)][urls.IndexOf(innerUrl)] = 1.0 / urlsPagePointTo.Count;
                    }
                    i.printOnLine("pages rows filled: " + (urls.IndexOf(url) + 1));
                }
            }
            return matrix;
        }

        public List<string> GetUrlsForMatrixValues(string url, List<string> urls)
        {
            
            var web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            var doc = web.Load(url);

            HashSet<string> urlsFromUrl = new HashSet<string>(doc.DocumentNode.Descendants("a")
                                              .Select(a => a.GetAttributeValue("href", null))
                                              .Where(u => !String.IsNullOrEmpty(u)).ToList());
            
            return urls.Where(x => urlsFromUrl.Contains(x)).ToList();
        }
    }
}
