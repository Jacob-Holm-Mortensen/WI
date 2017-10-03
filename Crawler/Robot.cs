using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngine
{
    class Robot
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "robotTXT\\");
        Program p = new Program();

        public bool IsUrlAllowed(string url)
        {
            Uri uri = new Uri(url);
            if (File.Exists(baseDir + uri.Host))
            {
                string[] temp = File.ReadAllText(baseDir + uri.Host).ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var restriction in temp)
                {
                    if (uri.PathAndQuery.Equals(restriction)) return false;
                }
            }
            return true;
        }

        // Save part of txt that is relevant for the crawler
        public void SaveRobotTXT(string robotUrl)
        {
            string content = p.GetFile(robotUrl);
            Directory.CreateDirectory(baseDir);
            Uri uri = new Uri(robotUrl);
            List<string> parsedContent = Parse(content)[1];
            if (!File.Exists(baseDir + uri.Host) && parsedContent.Count != 0)
            {
                File.WriteAllLines(baseDir + uri.Host + ".txt", parsedContent);
            }
        }

        public List<List<string>> Parse(string text)
        {
            List<List<string>> restrictionList = new List<List<string>>();
            List<string> AllowList = new List<string>(), DisallowList = new List<string>();
            int start = Regex.Match(text, @"(?i)User-agent: \*").Index + "User-agent: *".Length;
            string temp = text.Substring(start);
            int end = Regex.Match(temp, @"(?i)User-agent:|$").Index;
            temp = temp.Substring(0, end);
            foreach (Match match in Regex.Matches(temp, @"Allow: .*"))
            {
                AllowList.Add(match.ToString().Substring(7));
            }
            foreach (Match match in Regex.Matches(temp, @"Disallow: .*"))
            {
                DisallowList.Add(match.ToString().Substring(10));
            }
            restrictionList.Add(AllowList);
            restrictionList.Add(DisallowList);
            return restrictionList;
        }
    }
}
