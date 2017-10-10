using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SearchEngine
{
    class Crawler
    {
        HashSet<WebPage> backQueue = new HashSet<WebPage>();
        Queue<string> frontier = new Queue<string>();
        HashSet<string> frontierHashSet = new HashSet<string>();
        HashSet<HtmlStoring> locationMapping = new HashSet<HtmlStoring>();
        Program p = new Program();
        Robot r = new Robot();
        NearDuplicate near = new NearDuplicate();
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "");
        public HashSet<string> hosts = new HashSet<string>();
        int threadCount = 8;
        List<Thread> threads = new List<Thread>();

        // Runs the crawler process
        public void StartCrawling(string seed)
        {
            if (Directory.Exists(baseDir + "htmls\\"))
                foreach (var file in new DirectoryInfo(baseDir + "htmls\\").GetFiles()) file.Delete();
            if (Directory.Exists(baseDir + "robotTXT\\"))
                foreach (var file in new DirectoryInfo(baseDir + "RobotTXT\\").GetFiles()) file.Delete();
            DateTime start = DateTime.Now;
            foreach (var url in RetrieveUrls(seed)) AddToFrontier(url);

            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(new ThreadStart(threadContent));
                thread.IsBackground = true;
                threads.Add(thread);
                threads[threads.IndexOf(thread)].Start();
            }

            while (true)
            {
                Thread.Sleep(200);
                if (threads.All(x => !x.IsAlive)) break;
            }

            Console.WriteLine("Crawl duration: " + (DateTime.Now - start) + "\nHosts:");
            SaveLocationMapping();
            foreach (var host in hosts) Console.WriteLine(host);
        }

        public void threadContent()
        {
            while (backQueue.Count < 1000 && frontier.Count != 0)
            {
                string url;
                lock (frontier)
                {
                    // Look at the next url in the frontier
                    url = frontier.Dequeue();
                }
                frontierHashSet.Remove(url);
                // Move to back queue and insert new urls in the end of the frontier
                MoveWebPageToBackQueue(url);
            }
            Console.WriteLine("thread stropped");
        }

        // Adds urls to the back queue if the url does not exist
        public void AddToFrontier(string newUrl)
        {
            if (!backQueue.Any(webPage => webPage.url == newUrl) && !frontierHashSet.Any(url => url == newUrl))
            {
                frontier.Enqueue(newUrl);
                frontierHashSet.Add(newUrl);
            }
        }

        // Moves an url from the frontier to the back queue if the back queue does not contain a near duplicate
        public void MoveWebPageToBackQueue(string url)
        {
            string temp = p.GetFile(url);
            if (temp != null)
            {
                bool nearDup = false;
                /*foreach (var webPage in backQueue)
                {
                    if (near.NoHash(webPage.content, temp, 4)) nearDup = true;
                }*/
                if (!nearDup || backQueue.Count() == 0)
                {
                    foreach (var tempUrl in RetrieveUrls(url))
                    {
                        lock (backQueue)
                        {
                            lock (frontier)
                            {
                                AddToFrontier(tempUrl);
                            }
                        }
                    }
                    lock (backQueue)
                    {
                        backQueue.Add(new WebPage(url, temp));
                    }
                    SaveBackQueueElementToFile(url, temp);
                    Console.WriteLine("BackQueue: " + backQueue.Count() + ", Frontier: " + frontier.Count());
                }
            }
        }

        // Retrieves the urls of a web page
        public IEnumerable<string> RetrieveUrls(string url)
        {
            Uri uri = new Uri(url);
            if (!hosts.Contains(uri.Host))
            {
                hosts.Add(uri.Host);
                if (p.GetFile(uri.Scheme + "://" + uri.Host + "/robots.txt") != null)
                {
                    r.SaveRobotTXT(uri.Scheme + "://" + uri.Host + "/robots.txt");
                }
            }
            if (r.IsUrlAllowed(url))
            {
                var web = new HtmlWeb();
                web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
                try
                {
                    var doc = web.Load(url);
                    return CheckUrls(doc.DocumentNode.Descendants("a")
                                                  .Select(a => a.GetAttributeValue("href", null))
                                                  .Where(u => !String.IsNullOrEmpty(u)), uri);
                }
                catch (Exception)
                {

                }
            }
            return new HashSet<string>();
        }

        // Makes sure that urls are fully spelled out
        public HashSet<string> CheckUrls(IEnumerable<string> urls, Uri uri)
        {
            HashSet<string> newUrls = new HashSet<string>();
            foreach (var url in urls)
            {
                if (!url.StartsWith("#"))
                {
                    if (url.StartsWith("//"))
                    {
                        newUrls.Add(uri.Scheme + ":" + url);
                    }
                    else if(url.StartsWith("/"))
                    {
                        newUrls.Add(uri.Scheme + "://" + uri.Host + url);
                    }
                    else{
                        newUrls.Add(url);
                    }
                }
            }
            return newUrls;
        }

        public void SaveBackQueueElementToFile(string url, string content)
        {
            Directory.CreateDirectory(baseDir + "htmls\\");
            HtmlStoring temp = new HtmlStoring(url);
            if (!File.Exists(baseDir + "htmls\\" + temp.fileName))
            {
                File.WriteAllText(baseDir + "htmls\\" + temp.fileName, content);
                locationMapping.Add(temp);
            }
        }

        public void SaveLocationMapping()
        {
            Directory.CreateDirectory(baseDir + "htmls\\");
            List<string> mapping = new List<string>();
            foreach (var htmlStoring in locationMapping) mapping.Add("(" + htmlStoring.url + ") (" + htmlStoring.fileName + ")");
            if (!File.Exists(baseDir + "htmls\\mapping")) File.WriteAllLines(baseDir + "htmls\\mapping", mapping);
        }

        public string getUrlFromMapping(string fileName)
        {
            fileName = fileName.Replace(baseDir + "htmls\\", "");
            string url = "";
            if (File.Exists(baseDir + "htmls\\mapping"))
            {
                foreach (var line in File.ReadAllLines(baseDir + "htmls\\mapping"))
                {
                    if (line.Contains(fileName)) url = line.Split(null)[0].Replace("(", "").Replace(")", "");
                }
            }
            return url;
        }
    }
}