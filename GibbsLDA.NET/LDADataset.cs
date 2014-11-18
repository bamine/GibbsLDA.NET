using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class LDADataset
    {
        public WordDictionary LocalDictionary;
        public Document[] Docs;
        public int M; //number of documents
        public int V; //number of words

        public Dictionary<int, int> LocalIdToGlobalId;
        public WordDictionary GlobalDictionary; // link to the global dictionary, i.e. training dict for test set

        public LDADataset(int m)
        {
            LocalDictionary = new WordDictionary();
            M = m;
            V = 0;
            Docs = new Document[M];
        }

        public LDADataset(int m, WordDictionary globalDict)
            : this(m)
        {
            GlobalDictionary = globalDict;
            LocalIdToGlobalId = new Dictionary<int, int>();
        }

        public void SetDoc(Document doc, int idx)
        {
            if (idx >= 0 && idx < M)
            {
                Docs[idx] = doc;
            }
        }

        public void SetDoc(string doc, int idx)
        {
            if (idx >= 0 && idx < M)
            {
                var words = doc.Split();
                var ids = new List<int>();
                foreach (var word in words)
                {
                    int id = LocalDictionary.Size;

                    if (LocalDictionary.Contains(word))
                    {
                        id = LocalDictionary.GetId(word);
                    }

                    if (GlobalDictionary != null)
                    {
                        var gid = GlobalDictionary.GetId(word);

                        if (gid != -1)
                        {
                            LocalDictionary.AddWord(word);
                            LocalIdToGlobalId.Add(id, gid);
                            ids.Add(id);
                        }
                    }
                    else
                    {
                        LocalDictionary.AddWord(word);
                        ids.Add(id);
                    }
                }

                var document = new Document(ids.ToArray(), doc);
                Docs[idx] = document;
                V = LocalDictionary.Size;
            }
        }

        public static LDADataset ReadDataset(string filename)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    var line = reader.ReadLine();
                    var m = Convert.ToInt32(line);
                    var dataSet = new LDADataset(m);
                    for (int i = 0; i < m; i++)
                    {
                        line = reader.ReadLine();
                        dataSet.SetDoc(line, i);
                    }
                    return dataSet;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed reading dataSet :" + e.Message);
                return null;
            }
        }
        public static LDADataset ReadDataset(string filename, WordDictionary dictionary)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    var line = reader.ReadLine();
                    var m = Convert.ToInt32(line);
                    var dataSet = new LDADataset(m, dictionary);
                    for (int i = 0; i < m; i++)
                    {
                        line = reader.ReadLine();
                        dataSet.SetDoc(line, i);
                    }
                    return dataSet;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed reading dataSet :" + e.Message);
                return null;
            }
        }

        public static LDADataset ReadDataset(string[] strings)
        {
            var dataSet = new LDADataset(strings.Length);

            for (int i = 0; i < strings.Length; i++)
            {
                dataSet.SetDoc(strings[i], i);
            }

            return dataSet;
        }

        public static LDADataset ReadDataset(string[] strings, WordDictionary dictionary)
        {
            var dataSet = new LDADataset(strings.Length, dictionary);

            for (int i = 0; i < strings.Length; i++)
            {
                dataSet.SetDoc(strings[i], i);
            }

            return dataSet;
        }

    }

}
