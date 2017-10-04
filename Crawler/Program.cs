using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler c = new Crawler();
            Indexer i = new Indexer(c);
            Ranker r = new Ranker(c, i);
            /*string words = "";

            DateTime time = DateTime.Now;
            //c.StartCrawling("https://www.cbsnews.com/");
            var index = i.StartIndexing();
            Console.WriteLine("\n" + (DateTime.Now - time));

            while (!words.Equals("end"))
            {
                Console.WriteLine("type a word:");
                words = Console.ReadLine();
                r.PrintSearchResult(r.GetPagesWithWords(words, index), words);
            }*/

            List<List<double>> a = r.MakePageRankMatrix();
            Console.WriteLine();
            foreach (var item in a[1])
            {
                Console.Write(" " + item);
            }
            Console.ReadKey();
        }

        public string GetFile(string URL)
        {
            
            try
            {
                StreamReader reader;
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(URL);
                reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }
    }
}
