using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Crawler
{
    class Indexer
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "htmls\\");
        Crawler c = new Crawler();
        List<string> pageNames = new List<string>();
        List<List<string>> pageTermlist = new List<List<string>>();
        string[] stopwords;

        public Dictionary<string, Dictionary<int, double>> StartIndexing()
        {
            stopwords = File.ReadAllLines(baseDir.Replace("htmls\\", "") + "stopwords.txt");
            int count = 0;
            foreach (var fileName in Directory.GetFiles(baseDir))
            {
                if (!fileName.Equals(baseDir + "mapping")){
                    List<string> temp = RemoveStopWords(GetTerms(GetTextContent(File.ReadAllText(fileName))));
                    pageNames.Add(fileName);
                    pageTermlist.Add(temp);
                    printOnLine("processed pages: " + ++count + ", term count for current page: " + temp.Count);
                }
            }
            return IndexPages(pageTermlist);
        }
        
        public string GetTextContent(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            IEnumerable<string> text = new List<string>();

            try
            {
                text = htmlDoc.DocumentNode.SelectNodes("//body//text()").Select(node => node.InnerText);
            }
            catch (Exception e) { }
            StringBuilder output = new StringBuilder();
            foreach (string line in text)
            {
                output.AppendLine(line);
            }
            
            return HttpUtility.HtmlDecode(output.ToString());
        }

        public List<string> GetTerms(string content)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            content = rgx.Replace(content, "");
            return content.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public List<string> RemoveStopWords(List<string> terms)
        {
            List<string> updatedTerms = new List<string>();
            foreach (var term in terms)
            {
                if (!stopwords.Contains(term))
                {
                    updatedTerms.Add(term);
                }
            }
            return updatedTerms;
        }

        public Dictionary<string, Dictionary<int, double>> IndexPages(List<List<string>> pageTermList)
        {
            Dictionary<string, Dictionary<int, double>> index = new Dictionary<string, Dictionary<int, double>>();
            Console.WriteLine();
            foreach (var page in pageTermList)
            {
                foreach (var term in page)
                {
                    if (index.ContainsKey(term))
                    {
                        if (index[term].ContainsKey(pageTermList.IndexOf(page)))
                            index[term][pageTermList.IndexOf(page)]++;
                        else
                            index[term].Add(pageTermList.IndexOf(page), 1);
                    }
                    else index.Add(term, new Dictionary<int, double>() { { pageTermList.IndexOf(page), 1 } });
                }
                printOnLine("Indexed pages: " + (pageTermList.IndexOf(page) + 1));
            }
            return index;
        }

        public List<KeyValuePair<int, double>> GetPagesWithWords(string words, Dictionary<string, Dictionary<int, double>> index)
        {
            List<string> split = RemoveStopWords(words.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList());
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
            vectors = CreateVectors(output, tfidf);

            // Make vector comparison

            if (output.Count > 0)
            {
                pages = output[output.Keys.First()].ToList();
                foreach (var key in output.Keys.ToList())
                {
                    pages = pages.Where(x => output[key].Contains(x)).ToList();
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
            foreach (var page in pages)
            {
                vectors.Add(page, new List<double>());
                foreach (var term in tfidf)
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
                    if (pages.IndexOf(indexPage) < 10) Console.WriteLine(c.getUrlFromMapping(pageNames[indexPage.Key]));
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
                output.Add(key, Math.Log10(1000/input[key].Count()));
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

        public void printOnLine(string output)
        {
            Console.Write("\r");
            Console.Write(output + "     ");
        }
    }
}