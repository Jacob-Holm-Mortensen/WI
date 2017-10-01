using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    class WebPage
    {
        public string url, content;

        public WebPage(string _url, string _content)
        {
            url = _url;
            content = _content;
        }
    }
}
