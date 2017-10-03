using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class IndexPage
    {
        public int pageNr;
        public int specificTokenCount;

        public IndexPage(int _pageNr)
        {
            pageNr = _pageNr;
            specificTokenCount = 1;
        }

        public void increaseTokenCount()
        {
            specificTokenCount++;
        }
    }
}
