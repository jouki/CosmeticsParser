using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public class Language
    {
        public string languageName;
        public string languageCode;
        public static Language SelectedLanguage { get; set; }
        private static List<Language> _languages = new List<Language>()
        {
            new Language("English", "en"),
            new Language("German", "de"),
            new Language("Spanish", "es"),
            new Language("Spanish - Mexico", "es-MX"),
            new Language("French", "fr"),
            new Language("Italian", "it"),
            new Language("Japanese", "ja"),
            new Language("Korean", "ko"),
            new Language("Polish", "pl"),
            new Language("Portuguese - Brazil", "pt-BR"),
            new Language("Russian", "ru"),
            new Language("Thailand", "th"),
            new Language("Turkish", "tr"),
            new Language("Chinese - Simplified", "zh-Hans"),
            new Language("Chinese - Traditional", "zh-Hant"),
        };

        public Language(string langName, string langCode)
        {
            languageName = langName;
            languageCode = langCode;
        }

        public static Language SelectLanguage(int index)
        {
            SelectedLanguage = _languages[index - 1];
            return SelectedLanguage;
        }

        public static List<Language> GetLanguageList()
        {
            return _languages;
        }

        public static Language LanguageSelection()
        {
            int i = 0;
            Console.WriteLine("Select language:");
            GetLanguageList().ForEach(x => Console.WriteLine(String.Format("{0}. {1}", ++i, x.languageName)));
            while(true)
            {
                if(int.TryParse(Console.ReadLine().ToString(), out int lang) && lang > 0)
                {
                    return SelectLanguage(lang);
                }
                else if(lang == 0)
                {
                    Console.WriteLine("Special Feature SELECTED");
                    return null;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Wrong number. Select one of following languages by pressing entering number");
                    i = 0;
                    GetLanguageList().ForEach(x => Console.WriteLine(String.Format("{0}. {1}", ++i, x.languageName)));
                }
            }
        }

        public static bool IsSelectedEnglish()
        {
            return SelectedLanguage.languageCode.Equals("en");
        }
    }
}
