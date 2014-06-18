using App.Domain;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace App.Crawler
{
    public static class DidicionarioInformalCrawler
    {

        private static string xmlRepoPath = @"..\..\..\..\NLP\base\words.xml";
        private static string jsonRepoPath = @"..\..\..\..\NLP\base\words.json";
        private static ConcurrentDictionary<string, Word> wordDictionary = new ConcurrentDictionary<string, Word>();
        private static int wordLotSize = 0;
        private static int numberOfLots = 0;
        private static int numberOfWordsProccessed = 0;

        public static void Crawl()
        {
            LoadDictionary();

            List<Word> words = wordDictionary.Values.ToList();
            foreach (Word word in words)
            {
                if (!word.IsValid())
                {
                    System.Diagnostics.Debug.WriteLine(word.Token);

                    Word removedWord = null;
                    wordDictionary.TryRemove(word.Token, out removedWord);
                }
            }

            ProccessWords();

            SaveWords();
        }

        private static void LoadDictionary()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            WordRepository repository = new WordRepository();
            string serializedJson = File.ReadAllText(jsonRepoPath);
            repository = Newtonsoft.Json.JsonConvert.DeserializeObject<WordRepository>(serializedJson);

            foreach (Word word in repository.WordList)
            {
                if (!wordDictionary.ContainsKey(word.Token))
                {
                    word.GrammaticalClass = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<GrammaticalClassEnum, Grammar>>(word.GrammaticalClassAsJson);

                    wordDictionary.TryAdd(word.Token, word);
                }
            }


            watch.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("LoadDictionary: {0:D2}h{1:D2}m{2:D2}s", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds));
        }

        private static void ProccessWords()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            wordLotSize = 100;

            List<Word> listOfWords = wordDictionary.Values.ToList();
            //Queue<Thread> threadQueue = new Queue<Thread>();

            ////for (int i = 0; i < listOfWords.Count; i += wordLotSize)
            //for (int i = 0; i < 10000; i += wordLotSize)
            //{
            //    List<Word> listToProccess = new List<Word>();

            //    for (int j = i; j < i + wordLotSize; j++)
            //    {
            //        if (j < listOfWords.Count)
            //        {
            //            listToProccess.Add(listOfWords[j]);
            //        }
            //        else
            //        {
            //            break;
            //        }

            //    }

            //    Thread thread = new Thread(new ThreadStart(() => ProccessWordLot(listToProccess)));

            //    threadQueue.Enqueue(thread);

            //    thread.Start();                
            //}

            //Thread queuedThread = null;
            //while (threadQueue.Count > 0)
            //{
            //    queuedThread = threadQueue.Dequeue();
            //    queuedThread.Join();
            //}

            Parallel.ForEach(listOfWords, word => SearchWord(word));

            watch.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("ProccessWords: {0:D2}h{1:D2}m{2:D2}s", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds));
        }

        private static void ProccessWordLot(List<Word> words)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (Word word in words)
            {
                SearchWord(word);
                numberOfWordsProccessed++;
            }

            watch.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("ProccessWordLot(size={3}): {0}h{1}m{2}s", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds, words.Count));
        }


        private static string SearchWord(Word word)
        {
            int attempts = 3;
            while (attempts > 0)
            {
                try
                {
                    if (word.GrammaticalClass != null)
                    {
                        break;
                    }

                    string htmlString = new System.Net.WebClient().DownloadString(string.Format("http://www.dicionarioinformal.com.br/{0}/", word.Token));

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlString);

                    ClassifyWord(word, doc);

                    GetMoreWords(doc);

                    //System.Diagnostics.Debug.WriteLine(string.Format("http://www.dicionarioinformal.com.br/{0}/", word.Token));

                    break;
                }
                catch (Exception)
                {
                    attempts--;
                }
            }

            numberOfWordsProccessed++;

            if (attempts <= 0)
            {
                return "Error: " + word.Token;
            }
            else
            {
                return string.Format("http://www.dicionarioinformal.com.br/{0}", word.Token);
            }
        }

        private static void ClassifyWord(Word word, HtmlDocument doc)
        {
            var span = doc.DocumentNode.SelectSingleNode("//span[@class='textoDefinicao']");
            if (span != null)
            {
                List<HtmlNode> meaningfullNodes = span.ChildNodes.Where(node => node.Name == "#text").ToList();
                foreach (var node in meaningfullNodes)
                {
                    string innerText = node.InnerText.ToLower();

                    Grammar grammaticalClass = null;

                    //verifica as classes gramaticais que a palavra pode assumir
                    #region Classe Gramatical

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Substantivo.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Substantivo,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Artigo.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Artigo,
                        };
                    }
                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Adjetivo.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Adjetivo,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Numeral.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Numeral,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Pronome.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Pronome,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Verbo.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Verbo,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Advérbio.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Advérbio,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Preposição.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Preposição,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Conjunção.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Conjunção,
                        };
                    }

                    if (innerText.ToLower().LastIndexOf(GrammaticalClassEnum.Interjeição.ToString().ToLower()) > 0)
                    {
                        grammaticalClass = new Grammar()
                        {
                            GrammaticalClass = GrammaticalClassEnum.Interjeição,
                        };
                    }

                    #endregion
                    //verifica a variação de número da palavra
                    #region Variação de Número

                    if (grammaticalClass != null)
                    {
                        if (innerText.ToLower().LastIndexOf(NumberVariationEnum.Plural.ToString().ToLower()) > 0)
                        {
                            grammaticalClass.NumberVariation = NumberVariationEnum.Plural;
                        }

                        if (innerText.ToLower().LastIndexOf(NumberVariationEnum.Singular.ToString().ToLower()) > 0)
                        {
                            grammaticalClass.NumberVariation = NumberVariationEnum.Singular;
                        }
                    }

                    #endregion
                    //verifica o genero da palavra
                    #region Variação de Genero

                    if (grammaticalClass != null)
                    {
                        if (innerText.ToLower().LastIndexOf(GenderVariationEnum.Feminino.ToString().ToLower()) > 0)
                        {
                            grammaticalClass.GenderVariation = GenderVariationEnum.Feminino;
                        }

                        if (innerText.ToLower().LastIndexOf(GenderVariationEnum.Masculino.ToString().ToLower()) > 0)
                        {
                            grammaticalClass.GenderVariation = GenderVariationEnum.Masculino;
                        }
                    }

                    #endregion

                    if (grammaticalClass != null)
                    {
                        if (word.GrammaticalClass == null)
                        {
                            word.GrammaticalClass = new Dictionary<GrammaticalClassEnum, Grammar>();
                            word.GrammaticalClass.Add(grammaticalClass.GrammaticalClass, grammaticalClass);
                        }
                        else
                        {
                            if (!word.GrammaticalClass.ContainsKey(grammaticalClass.GrammaticalClass))
                            {
                                word.GrammaticalClass.Add(grammaticalClass.GrammaticalClass, grammaticalClass);
                            }
                        }

                        //System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(grammaticalClass));
                    }
                }
            }

            if (!wordDictionary.ContainsKey(word.Token))
            {
                wordDictionary.TryAdd(word.Token, word);
            }
            else
            {
                if (word.GrammaticalClass != null)
                {
                    foreach (GrammaticalClassEnum key in word.GrammaticalClass.Keys)
                    {
                        if (!wordDictionary[word.Token].GrammaticalClass.ContainsKey(key))
                        {
                            wordDictionary[word.Token].GrammaticalClass.Add(key, word.GrammaticalClass[key]);
                        }
                    }
                }
            }
        }

        private static void GetMoreWords(HtmlDocument doc)
        {
            HtmlNodeCollection palavrasRelacionadas = doc.DocumentNode.SelectNodes("//span[@class='sin_ant_rel']");

            if (palavrasRelacionadas != null)
            {
                foreach (HtmlNode palavra in palavrasRelacionadas)
                {
                    HtmlNodeCollection links = palavra.SelectNodes("//span[@class='sin_ant_rel']/a");
                    foreach (HtmlNode link in links)
                    {
                        string text = link.InnerText;
                        text = text.Replace("&nbsp;", "");
                        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\p{L}\p{N}]+", " ").ToLower();

                        string[] splittedtext = text.Split(new char[] { ' ' });

                        if (splittedtext != null && splittedtext.Length > 0)
                        {
                            foreach (Word word in splittedtext.Select(textPart => new Word(textPart)).ToList())
                            {
                                //Word word = new Word(textPart);
                                if (word.IsValid())
                                {
                                    if (!wordDictionary.ContainsKey(word.Token))
                                    {
                                        wordDictionary.TryAdd(word.Token, word);
                                        //System.Diagnostics.Debug.WriteLine("Nova palavras: " + textPart);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            HtmlNodeCollection exemploDefinicaoCollection = doc.DocumentNode.SelectNodes("//p[@class='ExemploDefinicao']");

            if (exemploDefinicaoCollection != null)
            {
                foreach (HtmlNode exemploDef in exemploDefinicaoCollection)
                {
                    string text = exemploDef.InnerText;
                    text = Regex.Replace(text, @"[^\p{L}\p{N}]+", " ").ToLower();

                    string[] splittedtext = text.Split(new char[] { ' ' });

                    foreach (var textPart in splittedtext)
                    {
                        if (!string.IsNullOrEmpty(textPart) && !wordDictionary.ContainsKey(textPart))
                        {
                            wordDictionary.TryAdd(textPart, new Word() { Token = textPart, GrammaticalClass = null });
                            //System.Diagnostics.Debug.WriteLine("Nova palavras: " + textPart);

                        }
                    }
                }
            }
        }

        private static void SaveWords()
        {

            //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(WordRepository);
            List<Word> listOfWords = wordDictionary.Values.OrderBy(w => w.Token).ToList();


            WordRepository repository = new WordRepository()
            {
                WordList = new List<Word>(),
            };

            foreach (Word word in listOfWords)
            {
                Dictionary<string, FriendlyGrammar> friendlyGrammaticalDictionary = null;

                if (word.GrammaticalClass != null)
                {
                    friendlyGrammaticalDictionary = new Dictionary<string, FriendlyGrammar>();
                    foreach (Grammar grammar in word.GrammaticalClass.Values)
                    {
                        friendlyGrammaticalDictionary.Add(grammar.GrammaticalClass.ToString(), new FriendlyGrammar()
                        {
                            GenderVariation = grammar.GenderVariation.ToString(),
                            NumberVariation = grammar.NumberVariation.ToString(),
                            GrammaticalClass = grammar.GrammaticalClass.ToString(),
                        });
                    }
                }

                Word repositoryWord = new Word()
                {
                    Token = word.Token,
                    GrammaticalClass = word.GrammaticalClass,
                    FriendlyGrammaticalClass = friendlyGrammaticalDictionary,
                    GrammaticalClassAsJson = Newtonsoft.Json.JsonConvert.SerializeObject(word.GrammaticalClass),
                };
                repository.WordList.Add(repositoryWord);
            }

            string serializedJson = Newtonsoft.Json.JsonConvert.SerializeObject(repository);
            File.WriteAllText(jsonRepoPath, serializedJson);

            //using (Stream stream = new FileStream(xmlRepoPath, FileMode.Create))
            //{
            //    System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(typeof(WordRepository));

            //    xmlserializer.Serialize(stream, repository);
            //}

            //string serializedXml = ServiceStack.Text.XmlSerializer.SerializeToString<WordRepository>(repository);
            //File.WriteAllText(repoPath, serializedXml);
        }

        //private static string RemoveAccents(this string text)
        //{
        //    StringBuilder sbReturn = new StringBuilder();
        //    var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
        //    foreach (char letter in arrayText)
        //    {
        //        if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(letter) != System.Globalization.UnicodeCategory.NonSpacingMark)
        //            sbReturn.Append(letter);
        //    }
        //    return sbReturn.ToString();
        //}
    }
}
