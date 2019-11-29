using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Project_Word;
using System.IO;
using System.Text.RegularExpressions;
using Classes;
using static System.Threading.Thread;
using static System.Random; 

namespace Dictionary
{
    class Program
    {
        public static string key = "dict.1.1.20191110T125921Z.2e938b8f1af39304.8ce369b76d519181943a5643717495fb5cacec21";

        static long GenID(ref List<Word> list)
        {
            long id;
            do
            {
                Random r = new Random();
                id = r.Next(1, 10000);
            } while (list.Select(w => w.id).Contains(id));
            return id; 
        }

        static void DescriptEngFromYandex(string word, ref YanWordEng en, ref YanWordEng ru, ref List<Word> w, ref List<Word> all, string trans, out bool err)
        {
            err = false;
            try
            {
                string request1 = @"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=" + key + @"&lang=en-en&text=" + word;
                //string request2 = @"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=" + key + @"&lang=en-ru&text=" + word;
                string response1 = Methods.Request(request1);
                //string response2 = Methods.Request(request2);
                en = Methods.DeSerializationObjFromStr<YanWordEng>(response1);
                //ru = Methods.DeSerializationObjFromStr<YanWordEng>(response2);
                /*foreach (var def in ru.GetDef())
                {
                    long id = GenID(ref all); 
                    var type = def.GetPos(); 
                    var syn = en.GetDef().First().GetTr().First().GetSyn().Select(x => x.GetText()).ToList();
                    var rus = def.GetText();
                    var meanE = en.GetDef().First().GetTr().Select(x => x.GetText()).ToList();
                    var meanR = def.GetTr().Select(e => e.GetText()).ToList();
                    var tr = en.GetDef().First().GetTs();
                    if (tr == null)
                        tr = trans;
                    var eng_ex = en.GetDef().First().GetTr().First().GetEx().Select(ex => ex.GetText()).ToList();
                    w.Add(new Word(id, type, word, syn, rus, meanE, meanR, tr, eng_ex, -1, new HashSet<string>()));
                }*/
            }
            catch { err = true; }
        }

        static void Main(string[] args)
        {
            var jf_word = new DataContractJsonSerializer(typeof(List<Word>));
            var jf_yan = new DataContractJsonSerializer(typeof(List<YanWordEng>));
            //using (var fs = new FileStream("dict.json", FileMode.Create))

            //var writeAsWord = File.CreateText("our_dict.json");
            var writeAsYan = File.CreateText("yan_dict_en-en.json");
            //var writeAsRus = File.CreateText("yan_dict_en-ru.json");
            string[] base_words = File.ReadAllLines("5000.txt");
            List<Word> list = new List<Word>();
            List<YanWordEng> yan_list_en = new List<YanWordEng>();
            //List<YanWordEng> yan_list_ru = new List<YanWordEng>(); 
            Console.WriteLine("Serializers, files and lists were created!");
            int i = 1;
            foreach (var line in base_words)
            {
                try
                {
                    string word = Regex.Match(line, @"\b[a-z]+\b").Value;
                    Console.WriteLine("We try to get english def of '" + word + "'");
                    long id;
                    bool f = long.TryParse(Regex.Match(line, @"^\d+\b").Value, out id);
                    if (!f)
                        Console.WriteLine("Bad word in: '" + line + "'");
                    string trans = Regex.Match(line, @"\[\w*\]").Value;
                    trans = trans.Substring(1, trans.Length - 2);
                    YanWordEng en = null;
                    YanWordEng ru = null;
                    List<Word> w = new List<Word>();
                    bool err;
                    DescriptEngFromYandex(word, ref en, ref ru, ref w, ref list, trans, out err);
                    if (!err && en != null)
                    {
                        list.AddRange(w);
                        yan_list_en.Add(en);
                        //yan_list_ru.Add(ru); 
                        Console.WriteLine("Wrote word #" + i);
                    }
                    else
                    {
                        continue;
                    }
                    ++i;
                    Sleep(1);
                }
                catch { }
            }
            /*try
            {
                jf_word.WriteObject(writeAsWord.BaseStream, list);
            }
            catch { Console.WriteLine("Error in writing our words"); }*/
            try
            {
                jf_yan.WriteObject(writeAsYan.BaseStream, yan_list_en);
            }
            catch { Console.WriteLine("Error in writein english words from yandex"); }
            /*try
            {
                jf_yan.WriteObject(writeAsRus.BaseStream, yan_list_ru);
            }
            catch { Console.WriteLine("Error in writing russian words from yandex"); }*/
            Console.WriteLine("Our lists were written down into files!");
            Console.ReadLine();
        }
    }
}
