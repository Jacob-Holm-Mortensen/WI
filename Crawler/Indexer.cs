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

namespace SearchEngine
{
    class Indexer
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "htmls\\");
        Crawler c;
        public List<string> pageNames = new List<string>();
        List<List<string>> pageTermlist = new List<List<string>>();
        string[] stopwords;

        public Indexer(Crawler _c)
        {
            c = _c;
        }

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
        
        public void printOnLine(string output)
        {
            Console.Write("\r");
            Console.Write(output + "     ");
        }
    }
}