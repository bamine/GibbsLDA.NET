using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class Document
    {
        public int[] Words;
        public String RawDocument;
        public int Length;

        public Document()
        {
            Words = null;
            RawDocument = "";
            Length = 0;
        }

        public Document(int length, int[] words)
        {
            Words = new int[words.Length];
            Array.Copy(words, Words, words.Length);
            Length = length;
        }

        public Document(int length, int[] words, string rawDocument)
        {
            Words = new int[words.Length];
            Array.Copy(words, Words, words.Length);
            Length = length;
            RawDocument = rawDocument;
        }

        public Document(IEnumerable<int> ids, string rawDocument)
            : this(ids.Count(), ids.ToArray(), rawDocument)
        {

        }

        public Document(IEnumerable<int> ids)
            : this(ids, "")
        {

        }


    }
}
