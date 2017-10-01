using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Crawler
{
    class Indexer
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "htmls\\");
        Crawler c = new Crawler();
        List<string> pageNames = new List<string>();
        string[] stopwords;

        public Dictionary<string, Dictionary<int, int>> StartIndexing()
        {
            stopwords = File.ReadAllLines(baseDir.Replace("htmls\\", "") + "stopwords.txt");
            int count = 0;
            List<List<string>> pages = new List<List<string>>();
            foreach (var fileName in Directory.GetFiles(baseDir))
            {
                if (!fileName.Equals(baseDir + "mapping")){
                    List<string> temp = RemoveStopWords(GetTokens(GetTextContent(File.ReadAllText(fileName))));
                    pageNames.Add(fileName);
                    pages.Add(temp);
                    printOnLine("processed pages: " + ++count + ", token count for current page: " + temp.Count);
                }
            }
            return IndexPages(pages);
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

        public List<string> GetTokens(string content)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            content = rgx.Replace(content, "");
            return content.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public List<string> RemoveStopWords(List<string> tokens)
        {
            List<string> updatedTokens = new List<string>();
            foreach (var token in tokens)
            {
                if (!stopwords.Contains(token))
                {
                    updatedTokens.Add(token);
                }
            }
            return updatedTokens;
        }

        public Dictionary<string, Dictionary<int, int>> IndexPages(List<List<string>> pageTokenList)
        {
            Dictionary<string, Dictionary<int, int>> index = new Dictionary<string, Dictionary<int, int>>();
            Console.WriteLine();
            foreach (var page in pageTokenList)
            {
                foreach (var token in page)
                {
                    if (index.ContainsKey(token))
                    {
                        if (index[token].ContainsKey(pageTokenList.IndexOf(page)))
                            index[token][pageTokenList.IndexOf(page)]++;
                        else
                            index[token].Add(pageTokenList.IndexOf(page), 1);
                    }
                    else index.Add(token, new Dictionary<int, int>() { { pageTokenList.IndexOf(page), 1 } });
                }
                printOnLine("Indexed pages: " + (pageTokenList.IndexOf(page) + 1));
            }
            return index;
        }

        public void PrintPagesWithWords(string words, Dictionary<string, Dictionary<int, int>> index)
        {
            List<string> split = RemoveStopWords(words.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList());
            Dictionary<string, Dictionary<int, int>> tf, idf, tfidf = new Dictionary<string, Dictionary<int, int>>();
            

            index = index.Where(x => split.Any(z => z == x.Key)).ToDictionary(x => x.Key, x => x.Value);
            // calc tf
            // calc idf
            // calc tfidf
            // compare search to tfidf as vectors








            Dictionary<int, int> result = new Dictionary<int, int>();
            List<Dictionary<int, int>> output = new List<Dictionary<int, int>>();
            foreach (var word in split)
            {
                index.TryGetValue(word, out result);
                output.Add(result);
            }
            if (output[0] != null)
            {
                result = output[0];
                foreach (var search in output)
                {
                    result = result.Where(x => search.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                }
                foreach (var indexPage in result)
                {
                    Console.WriteLine(c.getUrlFromMapping(pageNames[indexPage.Key]));
                }
                Console.WriteLine("Of the crawled pages " + result.Count + " contained the words (-stopwords): " + words);
                result = WithWeight(index.Where(x => split.Any(z => z == x.Key) && x.Value.Any(y => result.ContainsKey(y.Key))).ToDictionary(x => x.Key, x => x.Value));
            }
            else Console.WriteLine("No match");
            
        }

        public Dictionary<int, int> WithWeight(Dictionary<string, Dictionary<int, int>> matches)
        {
            foreach (var Value in matches.Values.ToList())
            {
                foreach (var count in Value.Values.ToList())
                {

                }
            }

            return new Dictionary<int, int>();
        }

        public void printOnLine(string output)
        {
            Console.Write("\r");
            Console.Write(output + "     ");
        }
    }
}