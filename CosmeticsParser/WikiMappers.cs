using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CosmeticsParser
{
    public static class WikiMappers
    {
        //Killers
        private static Dictionary<int, string> _killers;
        public static Dictionary<int, string> killers
        {
            get
            {
                if(_killers == null)
                {
                    _killers = PopulateWikiCharTable("killers");
                }

                return _killers;
            }
        }

        //Survivors
        private static Dictionary<int, string> _survivors;
        public static Dictionary<int, string> survivors {
            get
            {
                if(_survivors == null)
                {
                   _survivors = PopulateWikiCharTable("survivors");
                }

                return _survivors;
            }
        }

        private static Dictionary<int, string> PopulateWikiCharTable(string tableName)
        {
            var url = String.Format(@"https://deadbydaylight.fandom.com/api.php?action=scribunto-console&title=Module:X&question=require(%22Module:Datatable%22);mw.log(mw.text.jsonEncode(" + tableName + @"))&format=json");
            var rawJson = Encoding.UTF8.GetString(new WebClient().DownloadData(url));
            var result = ((List<dynamic>) JsonHelper.Deserialize(JsonHelper.Deserialize(rawJson)["print"]))
                .ToDictionary(
                x => (int) x["id"],
                x => (string) ((tableName.Equals("killers") ? "The " : string.Empty) + (x.ContainsKey("dbdName") ? x["dbdName"] : x["name"]))
            );

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

        

    }
}
