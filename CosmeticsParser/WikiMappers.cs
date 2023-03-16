using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace CosmeticsParser
{
    public static class WikiMappers
    {
        private static int _articleCounter = 1; //used for informative output
        private static int _loadedAPIKillers = 0;
        private static string _wikiLangCode;
        public static string WikiLangCode
        {
            get
            {
                if(_wikiLangCode == null)
                {
                    _wikiLangCode = GetLanguageCodeWikiMapping(Language.SelectedLanguage);
                }

                return _wikiLangCode;
            }
        }
        //Killers
        private static Dictionary<int, string> _killers;
        public static Dictionary<int, string> Killers
        {
            get
            {
                if(_killers == null)
                {
                    Console.WriteLine("Processing Wiki Table Killers...");
                    _killers = PopulateWikiCharTable("killers");
                }

                return _killers;
            }
        }

        //Survivors
        private static Dictionary<int, string> _survivors;
        public static Dictionary<int, string> Survivors {
            get
            {
                if(_survivors == null)
                {
                    Console.WriteLine("Processing Wiki Table Survivors...");
                    _survivors = PopulateWikiCharTable("survivors");
                }

                return _survivors;
            }
        }

        private static Dictionary<int, string> PopulateWikiCharTable(string tableName)
        {
            var url = BuildWikiApiLink(Module.Datatable, "mw.text.jsonEncode(" + tableName + ")");
            //String.Format(@"https://deadbydaylight.fandom.com/api.php?action=scribunto-console&title=Module:X&question=require(%22Module:Datatable%22);mw.log(mw.text.jsonEncode(" + tableName + @"))&format=json");
            var rawJson = Encoding.UTF8.GetString(new WebClient().DownloadData(url));
            var deserealizedJson = (List<dynamic>) JsonHelper.Deserialize(JsonHelper.Deserialize(rawJson)["print"]);
            _loadedAPIKillers = deserealizedJson.Count;
            var result = deserealizedJson.ToDictionary(
                x => (int) x["id"],
                x => (string) ((tableName.Equals("killers") ? GetArticle(x) : string.Empty) + (x.ContainsKey("dbdName") ? x["dbdName"] : x["name"]))
            );

            if(tableName.Equals("killers")) Console.WriteLine();
            return result;
        }

        public static List<BodyType> outfits = new List<BodyType>() { BodyType.Outfit };
        public static List<BodyType> heads = new List<BodyType>() { BodyType.SurvivorHead, BodyType.KillerHead };
        public static List<BodyType> torsos = new List<BodyType>() { BodyType.SurvivorTorso };
        public static List<BodyType> legs = new List<BodyType>() { BodyType.SurvivorLegs, BodyType.KillerLegs };
        public static List<BodyType> bodies = new List<BodyType>() { BodyType.KillerBody };
        public static List<BodyType> weapons = new List<BodyType>() { BodyType.KillerWeapon };
        public static List<BodyType> charms = new List<BodyType>() { BodyType.Charm };
        public static List<BodyType> masks = new List<BodyType>() { BodyType.Mask };
        public static List<BodyType> hairs = new List<BodyType>() { BodyType.Hair };
        public static List<BodyType> arms = new List<BodyType>() { BodyType.Arm };
        public static List<BodyType> hands = new List<BodyType>() { BodyType.Hand };
        public static List<BodyType> upperBodies = new List<BodyType>() { BodyType.UpperBody };
        public static List<List<BodyType>> bodyTypes = new List<List<BodyType>>() { outfits, heads, masks, hairs, torsos, legs, bodies, weapons, charms, arms, hands, upperBodies };

        public static string GetTableNameByBodyType(BodyType bodyType)
        {
            switch(bodyType)
            {
                case BodyType.KillerHead:
                case BodyType.SurvivorHead: return "heads";
                case BodyType.SurvivorTorso: return "torsos";
                case BodyType.KillerLegs:
                case BodyType.SurvivorLegs: return "legs";
                case BodyType.KillerWeapon: return "weapons";
                case BodyType.KillerBody: return "bodies";
                case BodyType.Outfit: return "outfits";
                case BodyType.Charm: return "charms";
                case BodyType.Mask: return "masks";
                case BodyType.Hair: return "hairs";
                case BodyType.Arm: return "arms";
                case BodyType.Hand: return "hands";
                case BodyType.UpperBody: return "upperBodies";
                    //case BodyType.KillerLegs: return "upperBodies";
            }
            return "#NAN#";
        }

        public static string GetLanguageCodeWikiMapping(Language lang)
        {
            //should trim the content of langCode after dash if there is one
            return lang.languageCode.Substring(0, lang.languageCode.IndexOf("-") + lang.languageCode.Length + 1);
        }

        public static string BuildWikiApiLink(Module module, string operation = "")
        {
            var result = string.Empty;
            string wikiBaseLink = "https://deadbydaylight.fandom.com/";
            string wikiApiLinkPart = "api.php?action=scribunto-console&title=Module:X&question=";
            string consoleQuery = String.Format(
                "mod=require(%22Module:{0}%22);" +
                "mw.log({1})",
                GetMappedWikiModule(module), operation
            );
            string outputFormat = "&format=json";

            result = 
                wikiBaseLink + 
                (Language.IsSelectedEnglish() ? string.Empty : WikiLangCode + "/") +
                wikiApiLinkPart +
                consoleQuery +
                outputFormat;

            return result;
        }

        public static string GetMappedWikiModule(Module module)
        {
            return module.ToString().Replace("_", "/");
        }

        public static string GetArticle(dynamic obj)
        {
            Console.Write("\rRetrieving Wiki logic for killer's larticle... [{0}/{1}]", _articleCounter, _loadedAPIKillers);
            if(Language.IsSelectedEnglish())
            {
                _articleCounter++;
                return "The ";
            }
            var result = string.Empty;
            if(obj.ContainsKey("article"))
            {
                if(bool.TryParse(obj["article"], out bool success)){
                    //Currently Unused
                }
                else
                {
                    var url = BuildWikiApiLink(Module.Languages, "mod.article_" + WikiLangCode + @"({article=""" + obj["article"] + @"""})");
                    string rawJson = Encoding.UTF8.GetString(new WebClient().DownloadData(url));
                    result = JsonHelper.Deserialize(rawJson)["print"].Replace("\n", string.Empty);
                }
            }
            _articleCounter++;
            return result;
        }
        

    }
}
