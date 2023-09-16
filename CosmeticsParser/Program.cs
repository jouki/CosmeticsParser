using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CosmeticsParser
{
    class Program
    {
        private List<string> _outfitPieceNames = new List<string>();
        public static string LinkBase = "https://dbd-info.com/api/";

        [STAThread]
        static void Main(string[] args)
        {            
            Introduction();
            var test = Language.LanguageSelection();
            if(test == null)
            {
                SpecialFeature();
                return;
            }
            Console.Clear();
            Console.WriteLine("Processing...");

            var cosmeticsJson = GetDataFromUrl(LinkBase + "cosmetics");
            var wikiJson = Encoding.UTF8.GetString(new WebClient().DownloadData("https://deadbydaylight.fandom.com/api.php?action=scribunto-console&title=Module:API&question==require(%27Module:API%27).getData()&format=json"));
            Dictionary<string, dynamic> cosmetics = JsonHelper.Deserialize(cosmeticsJson)["data"]; //JSON now returns success flag and error list. We want only "data" section
            Dictionary<string, dynamic> wikiChars = JsonHelper.Deserialize(wikiJson);
            var resultString = wikiChars["return"];
            List<dynamic> resultJson = JsonHelper.Deserialize(resultString);
            Dictionary<dynamic, dynamic> wikiCharsRaw = ((List<dynamic>) JsonHelper.Deserialize(JsonHelper.Deserialize(wikiJson)["return"])).ToDictionary(x => x["name"], x => x);


            var filteredCosmetics = cosmetics.Values.Where(x => x.ContainsKey("Character") && IsNotCurrency(x)).ToList(); //Removing any element that has no assignment of Character //excluding charms?
            var CosList = new List<Cosmetic>();
            filteredCosmetics.ForEach(filtered => CosList.Add(new Cosmetic(filtered)));

            //TODO REMOVE? maybe not needed at all
            //var charmCollection = cosmetics.Values.Where(x => x.ContainsKey("Type") && x["Type"] == "Charm").ToList();
            //var charms = new List<Cosmetic>();
            //charmCollection.ForEach(filtered => charms.Add(new Cosmetic(filtered)));

            var allCosmetics = new Dictionary<List<BodyType>, List<Cosmetic>>();
            WikiMappers.bodyTypes.ForEach(
                x => allCosmetics.Add(
                    x,
                    CosList.Where(y => x.Contains(y.type)).ToList()));

            Dictionary<string, string> wikiStrings = new Dictionary<string, string>();
            foreach(var bodyTypeCategory in WikiMappers.bodyTypes)
            {
                var representative = bodyTypeCategory.First(); //since several categories are merged we simply take first bodyType
                wikiStrings.Add(WikiMappers.GetTableNameByBodyType(representative), GenerateTable(allCosmetics, representative));
            }

            //Check
            var collectionItemsWithoutOutfit = CosList.Where(cos => CosList.Where(x => x.type == BodyType.Outfit).SelectMany(x => x.outfitItems).Contains(cos.cosmeticId)).ToList().Where(x => x.collectionName != null && x.collectionName.Equals(string.Empty)).ToList();

            //To be removed
            var regex = new Regex(@"\w+?_([a-zA-Z]+|[0-9]+).+");
            var idsTypes = new List<string>();
            CosList.Where(x => new BodyType[] { BodyType.SurvivorHead, BodyType.KillerHead, BodyType.SurvivorTorso, BodyType.KillerBody }.Contains(x.type)).Select(x => x.cosmeticId).ToList().ForEach(x => idsTypes.Add(regex.Match(x).Groups[1].Value));
            idsTypes = idsTypes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            wikiStrings.Add("collections", GetCollectionListString());

            //Check
            var riftRewardsNotMapped = Rift.rifts.SelectMany(x => x.tiers.Select(t => t.rewardId)).ToList().Where(x => !CosList.Where(y => y.riftTier > -1).Select(y => y.cosmeticId).Contains(x)).Distinct().ToList();


            //Final Processing
            var timeOffsetString = DateTimeOffset.Now.Offset.ToString();
            var timeOffset = long.Parse(timeOffsetString.Substring(0, timeOffsetString.IndexOf(":")));
            var region = RegionInfo.CurrentRegion.ThreeLetterISORegionName;
            var utc = DateTime.UtcNow;
            var result = string.Join("\n", wikiStrings.Values);
            result = 
                String.Format("--Current timestamp: {0} ({1} {2}) [UTC:{3}]", DateTime.Now.ToString(), region, timeOffset.ToString("+#;-#;0"), utc) + Environment.NewLine +
                String.Format("--Language: {0}-{1}", Language.SelectedLanguage.languageName, Language.SelectedLanguage.languageCode) + Environment.NewLine +
                result;
            try
            {
                Clipboard.SetDataObject(result);
                //Clipboard.SetText(result);
            }
            catch (Exception ex)
            {
                //Clipboard.SetDataObject(result);
            }
            StoreAndOpenDataInFile(result);
        }

        private static void StoreAndOpenDataInFile(string result)
        {
            string file = String.Format(@"CP_{0}.txt", Language.SelectedLanguage.languageCode);
            File.WriteAllText(file, result);
            Process.Start(file);
        }

        private static bool IsNotCurrency(dynamic obj)
        {
            return obj.ContainsKey("Type") && !obj["Type"].Equals("Currency");
        }

        private static void Introduction()
        {
            Console.WriteLine("Welcome! The script will parse the Cosmetics JSON data from dbd-info.com");
            Console.WriteLine("Be aware the the following script override your clipboard with data.\n");
        }

        public static string GetDataFromUrl(string link)
        {
            var webClient = new WebClient();
            var result = String.Empty;
            webClient.Headers.Add(HttpRequestHeader.Cookie, String.Format("language={0}", Language.SelectedLanguage.languageCode));

            return Encoding.UTF8.GetString(webClient.DownloadData(link));
        }
        private static string GetCollectionListString()
        {
            var result = "p.collections = {\n";

            for(int i = 0; i < Cosmetic._collections.Count; i++)
            {
                var collectionName = Cosmetic._collections[i];
                result += string.Format("\t" + @"{{id = {0}, name = {1}}}" + (i + 1 < Cosmetic._collections.Count ? "," : string.Empty) + "\n",
                    i + 1,
                    Utils.RefactorName(collectionName)
                );
            }
            result += "}\n";
            return result;
        }

        private static string GenerateTable(Dictionary<List<BodyType>, List<Cosmetic>> allCosmetics, BodyType bodyType)
        {
            string result = "p." + WikiMappers.GetTableNameByBodyType(bodyType) + " = {\n";
            var resultObjs = allCosmetics.Single(x => x.Key.Contains(bodyType)).Value;

            int i = 1;
            for(i = 1; i <= resultObjs.Count; i++) //index shifted as per a LUA indexing
            {
                var cosmetic = resultObjs[i - 1];
                var outfitPieces = string.Empty;
                var charType = string.Empty;
                var rarity = string.Empty;
                var linkedSet = string.Empty;
                //TODO: tome
                if(bodyType == BodyType.Outfit || bodyType == BodyType.Charm)
                {
                    rarity = @", rarity = " + (int) cosmetic.rarity; //rarity for outfits and charms
                    linkedSet = cosmetic.linkedSet ? @", linked = " + cosmetic.linkedSet.ToString().ToLower() : string.Empty;

                }
                if(bodyType == BodyType.Outfit)
                {
                    outfitPieces = GetOutfitPiecesString(allCosmetics, cosmetic);
                    charType = string.Format(", {0} = {1}", (cosmetic.charType == CharType.K ? "killer" : "survivor"), cosmetic.character.wikiID); //we want the charType only at outfits, otherwise we keep the variable empty
                }
                if(bodyType == BodyType.Charm && cosmetic.charType != CharType.B) //if the charm is universal we won't store a value at all
                {
                    charType = string.Format(", {0} = true", (cosmetic.charType == CharType.K ? "killer" : "survivor")); //charm version
                }
                var desc = (!new string[] { "\t", "" }.Contains(cosmetic.description) ? @", desc = " + Utils.RefactorText(cosmetic.description.Trim()) : String.Empty);
                var cellsPrice = (cosmetic.prices.Keys.Contains(Currency.Cells) ? @", ac = " + cosmetic.prices[Currency.Cells] : string.Empty);
                var shardsPrice = (cosmetic.prices.Keys.Contains(Currency.Shards) ? @", is = " + cosmetic.prices[Currency.Shards] : string.Empty);
                var collection = cosmetic.collectionId != -1 ? ", collectionId = " + cosmetic.collectionId : string.Empty;
                var filename = @", filename = """ + cosmetic.cosmeticId + @".png""";
                var riftTier = cosmetic.riftTier != -1 ? ", tome = " + cosmetic.rift.id + ", tier = " + cosmetic.riftTier : string.Empty;
                var rDate = cosmetic.startDate != null ? @", rDate = """ + cosmetic.startDate.ToString("dd.MM.yyyy") + @"""" : string.Empty;
                var purchasable = @", purchasable = " + cosmetic.purchasable.ToString().ToLower();
                result += String.Format("\t" + @"{{id = {0}, name = {1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}}}" + ((i < resultObjs.Count || bodyType == BodyType.Outfit) ? "," : string.Empty) + "\n",
                    i,
                    Utils.RefactorName(cosmetic.cosmeticName.Trim()),
                    rarity,
                    charType,
                    linkedSet,
                    outfitPieces,
                    collection,
                    cellsPrice,
                    shardsPrice,
                    purchasable,
                    rDate,
                    riftTier,
                    filename,
                    desc
                );
            }

            if(bodyType == BodyType.Outfit)
            {
                result += AppendFakeOutfits(allCosmetics, ++i);
            }
            result += "}" + (bodyType == BodyType.Outfit ? "\n" : string.Empty);

            return result;
        }

        private static string AppendFakeOutfits(Dictionary<List<BodyType>, List<Cosmetic>> allCosmetics, int continuedIndex)
        {
            var result = "\n";
            var outfitlessCosmeticsStringList = allCosmetics.Single(x => x.Key.Contains(BodyType.Outfit)).Value.SelectMany(x => x.outfitItems).ToList();
            //var foundOutfitPieces = allCosmetics.Where(x => !x.Key.Contains(BodyType.Outfit)).SelectMany(x => x.Value).ToList().Where(x => outfitlessCosmeticsStringList.Contains(x.cosmeticId)).ToList();
            var outfitlessCosmeticsList = allCosmetics.Where(x => !x.Key.Contains(BodyType.Outfit) && !x.Key.Contains(BodyType.Charm)).SelectMany(x => x.Value).ToList().Where(x => !outfitlessCosmeticsStringList.Contains(x.cosmeticId)).ToList();
            var piecesWithoutCharacter = outfitlessCosmeticsList.Where(x => x.character.wikiID == -1).ToList();

            var log = String.Empty;
            piecesWithoutCharacter.ForEach(x => log += "cosmeticId: " + x.cosmeticId + ", cosmeticName: " + x.cosmeticName + ", type: " + x.type.ToString() + "\n");

            var filteredOutfitlessCosmeticsList = outfitlessCosmeticsList.Where(x => x.character.wikiID != -1).ToList();
            for(int i = 0; i < filteredOutfitlessCosmeticsList.Count; i++ )
            {
                var cosmetic = filteredOutfitlessCosmeticsList[i];
                var pieces = String.Format(", pieces = {{{0}}}", WikiMappers.GetTableNameByBodyType(cosmetic.type) + " = " + (allCosmetics.Single(x => x.Key.Contains(cosmetic.type)).Value.IndexOf(cosmetic) + 1));
                var character = String.Format(", {0} = {1}", (cosmetic.charType == CharType.K ? "killer" : "survivor"), cosmetic.character.wikiID);
                var collection = cosmetic.collectionId != -1 ? ", collectionId = " + cosmetic.collectionId : string.Empty; //currently there's none cosmetic piece that'd be in collection but not part of an outfit
                var rarity = String.Format(", rarity = {0}", (int) cosmetic.rarity);
                var fakeOutfit = ", fakeOutfit = true";
                var purchasable = @", purchasable = " + cosmetic.purchasable.ToString().ToLower();
                result += String.Format("\t" + @"{{id = {0}{1}{2}{3}{4}{5}{6}}}" + (i + 1 < filteredOutfitlessCosmeticsList.Count ? "," : string.Empty) + "\n",
                    continuedIndex++,
                    character,
                    rarity,
                    fakeOutfit,
                    purchasable,
                    collection,
                    pieces
                );
            }
            return result;
        }

        private static string GetOutfitPiecesString(Dictionary<List<BodyType>, List<Cosmetic>> allCosmetics, Cosmetic cosmetic)
        {
            var result = ", pieces = {";
            var outfitPieces = GetOutfitPiecesWithIndexes(allCosmetics, cosmetic);

            result += string.Join(", ", outfitPieces.Select(outfitPiece => string.Format(@"{0} = {1}", WikiMappers.GetTableNameByBodyType(outfitPiece.Key.Key), outfitPiece.Key.Value)).ToList());
            result += "}";


            return result;
        }

        private static Dictionary<KeyValuePair<BodyType, int>, Cosmetic> GetOutfitPiecesWithIndexes(Dictionary<List<BodyType>, List<Cosmetic>> allCosmetics, Cosmetic cosmetic)
        {
            var outfitPieces = new Dictionary<KeyValuePair<BodyType, int>, Cosmetic>();
            var excludedBodyTypes = new List<BodyType>() { BodyType.Outfit }; // This serves for optimizing not to search through the lists that we don't want search in or already found the piece (there are no two same pieces in one outfit)

            foreach(var piece in cosmetic.outfitItems)
            {
                var filteredCosmeticList = allCosmetics.Where(x => !x.Key.Intersect(excludedBodyTypes).Any()).ToList();
                foreach(var filteredCosmetic in filteredCosmeticList)
                {
                    var foundPiece = filteredCosmetic.Value.FirstOrDefault(x => x.cosmeticId.Equals(piece));
                    if(foundPiece != null)
                    {

                        outfitPieces.Add(new KeyValuePair<BodyType, int>(foundPiece.type, filteredCosmetic.Value.IndexOf(foundPiece) + 1), foundPiece);
                        excludedBodyTypes.Add(foundPiece.type);
                        break;
                    }
                }
            }
            return outfitPieces;
        }


        private static void SpecialFeature()
        {



            Console.WriteLine("Program is at its end.");
            Console.Read();
        }
    }
}
