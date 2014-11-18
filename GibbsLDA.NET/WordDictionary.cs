using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class WordDictionary
    {
        public Dictionary<string, int> Word2Id;
        public Dictionary<int, string> Id2Word;
        public int Size
        {
            get
            {
                return Word2Id.Count;
            }
        }

        public WordDictionary()
        {
            Word2Id = new Dictionary<string, int>();
            Id2Word = new Dictionary<int, string>();
        }

        public string GetWord(int id)
        {
            return Id2Word[id];
        }

        public int GetId(string word)
        {
            return Word2Id.ContainsKey(word) ? Word2Id[word] : -1;
        }

        public bool Contains(int id)
        {
            return Id2Word.ContainsKey(id);
        }

        public bool Contains(string word)
        {
            return Word2Id.ContainsKey(word);
        }

        public int AddWord(string word)
        {
            if (!Contains(word))
            {
                int id = Size;
                Word2Id.Add(word, id);
                Id2Word.Add(id, word);
                return id;
            }
            return Word2Id[word];
        }

        public bool ReadWordMap(string wordMapFile)
        {
            try
            {
                using (StreamReader reader = new StreamReader(wordMapFile))
                {
                    string line = reader.ReadLine();
                    int nWords = Convert.ToInt32(line);
                    for (int i = 0; i < nWords; i++)
                    {
                        line = reader.ReadLine();
                        var parts = line.Split();
                        if (parts.Count() != 2) continue;
                        string word = parts[0];
                        int id = Convert.ToInt32(parts[1]);
                        Id2Word.Add(id, word);
                        Word2Id.Add(word, id);

                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while reading dictionary :" + e.Message);
                return false;
            }
        }

        public bool WriteWordMap(string wordMapFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(wordMapFile))
                {
                    writer.WriteLine(Size);
                    foreach (KeyValuePair<string, int> item in Word2Id)
                    {
                        writer.WriteLine(item.Key + " " + item.Value);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while writing dictionary :" + e.Message);
                return false;
            }
        }
    }
}
