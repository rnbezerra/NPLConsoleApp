using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace App
{
    class Program
    {

        private static string repoPath = @"C:\Users\Vtex\Downloads\NLP\br word db\words.xml";
        private static string jsonRepoPaht = @"C:\Users\Vtex\Downloads\NLP\br word db\words.json";
        private static HashSet<string> wordSet = new HashSet<string>();

        static void Main(string[] args)
        {
            //Proccess(@"C:\Users\Vtex\Downloads\NLP\br word db\palavras_uni.txt");

            //Proccess(@"C:\Users\Vtex\Downloads\NLP\br word db\palavras2.txt");

            ////System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(WordRepository);
            //WordRepository repository = new WordRepository()
            //{
            //    Word = wordSet.ToList(),
            //};

            //repository.Word = repository.Word.OrderBy(w => w).ToList();

            //string serializedJson = Newtonsoft.Json.JsonConvert.SerializeObject(repository);
            //string serializedXml = ServiceStack.Text.XmlSerializer.SerializeToString<WordRepository>(repository);
            //System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(WordRepository));

            //Stream stream = new FileStream(repoPath, FileMode.Create);

            //xmlserializer.Serialize(stream, repository);

            //stream.Close();

            //File.WriteAllText(repoPath, serializedXml);
            //File.WriteAllText(jsonRepoPah, serializedJson);

            Crawler.DidicionarioInformalCrawler.Crawl();
        }

        private static void Proccess(string path)
        {
            List<string> file = File.ReadAllLines(path).ToList();

            foreach (string line in file)
            {
                string word = line;

                if (word.LastIndexOf('/') > -1)
                {
                    word = word.Split(new char[] { '/' })[0];
                }

                if (!wordSet.Contains(word))
                {
                    wordSet.Add(word);
                }
            }
        }

    }
}
