using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string words = "";
            DateTime time = DateTime.Now;
            Crawler c = new Crawler();
            //c.StartCrawling("https://www.cbsnews.com/");
            Indexer i = new Indexer();
            var index = i.StartIndexing();
            Console.WriteLine("\n" + (DateTime.Now - time));
            while (!words.Equals("end"))
            {
                Console.WriteLine("type a word:");
                words = Console.ReadLine();
                i.PrintSearchResult(i.GetPagesWithWords(words, index), words);
            }
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
