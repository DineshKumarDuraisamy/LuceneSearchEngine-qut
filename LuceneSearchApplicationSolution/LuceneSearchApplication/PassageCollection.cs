using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneApplication
{
    class Passage
    {
        public int is_selected;
        public string url;
        public string passage_text;
        public int passage_ID;

    }
    class PassageCollection
    {
        public List<Passage> passages;
        public int query_id;
        public List<string> answers;
        public string query_type;
        public string query;
    }
}
