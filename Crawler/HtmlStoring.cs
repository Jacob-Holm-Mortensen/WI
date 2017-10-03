using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class HtmlStoring
    {
        public string url;
        public string fileName;

        public HtmlStoring(string _url)
        {
            url = _url;
            fileName = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+","").Replace("\\","").Replace("/","");
        }
    }
}
