using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public class Cosmetic : IComparable<Cosmetic>
    {
        public static List<string> _collections = new List<string>();
        private static Dictionary<string, Dictionary<BodyType, BodyType>> atypicalBodyTypeMapping =
            new Dictionary<string, Dictionary<BodyType, BodyType>>() {
                { "The Hillbilly", new Dictionary<BodyType, BodyType>()
                    {
                        { BodyType.KillerHead, BodyType.UpperBody },
                        { BodyType.KillerBody, BodyType.KillerLegs }
                    }
                },
                { "The Dredge", new Dictionary<BodyType, BodyType>()
                    {
                        { BodyType.KillerHead, BodyType.Arm }
                    }
                },
                { "Ashley J. Williams", new Dictionary<BodyType, BodyType>()
                    {
                        { BodyType.SurvivorTorso, BodyType.UpperBody }
                    }
                }
            };


        public string cosmeticId;
        public string cosmeticName;
        public string description;
        public string iconFilePathList;
        public string collectionName;
        public int collectionId;
        public string inclusionVersion;
        public BodyType type;
        public Character character;
        public bool linkedSet;
        public bool purchasable;
        public DateTime endDate;
        public DateTime startDate;
        public Rarity rarity;
        public List<string> outfitItems;
        public Dictionary<Currency, int> prices;
        public Rift rift;
        public int riftTier;
        public bool visceral;

        public KeyValuePair<int, int> mappedChar;
        public CharType charType;

        public Cosmetic(dynamic obj) {
            this.cosmeticId = obj["CosmeticId"];
            this.cosmeticName = obj["CosmeticName"].Trim();
            this.character = new Character((int) obj["Character"]);
            this.description = obj["Description"];
            this.iconFilePathList = obj["IconFilePathList"];
            this.collectionName = GetCollectionName(obj["CollectionName"]);
            var collectionIndex = _collections.IndexOf(this.collectionName);
            this.collectionId = collectionIndex != -1 ? collectionIndex + 1 : collectionIndex; //indexing for wiki/LUA table
            this.inclusionVersion = obj["InclusionVersion"];
            this.type = GetBodyType(obj);
            this.linkedSet = obj.ContainsKey("Unbreakable") ? obj["Unbreakable"] : false;
            this.purchasable = obj["Purchasable"];
            if(obj.ContainsKey("EndDate")) this.endDate = obj["EndDate"];
            if(obj.ContainsKey("StartDate")) this.startDate = obj["StartDate"];
            this.rarity = Enum.Parse(typeof(Rarity), obj["Rarity"]);
            this.outfitItems = obj.ContainsKey("OutfitItems") ? ((IEnumerable) obj["OutfitItems"]).Cast<string>().ToList() : null;//.Select(x => (string)x).ToList();

            var ccys = ((IEnumerable) obj["Prices"]).Cast<Dictionary<string, dynamic>>().SelectMany(x => x).ToList();
            //Dictionary<Currency, int> ccyDic = new Dictionary<Currency, int>();
            //foreach(var ccy in ccys) {
            //    ccyDic.Add((Currency) Enum.Parse(typeof(Currency), ccy.Key), (int) ccy.Value);
            //}

            this.prices = ccys.ToDictionary(ccy => ((Currency) Enum.Parse(typeof(Currency), ccy.Key)),
                                            ccy => (int) ccy.Value); //ccyDic;
            MapRift();
            this.visceral = obj.ContainsKey("Prefix") ? obj["Prefix"] == "Visceral" : false;
            MapChar();
            MapCharType(obj);
        }

        private string GetCollectionName(string collectionName)
        {
            if(collectionName == null)
            {
                return null;
            }
            else
            {
                collectionName = CultureInfo.GetCultures(CultureTypes.NeutralCultures).ToList().First(x => x.Name.Equals("en")).TextInfo.ToTitleCase(collectionName.Trim().ToLower()); ;
                if(!collectionName.Equals(string.Empty) && !_collections.Contains(collectionName))
                {
                    _collections.Add(collectionName);
                }
                return collectionName;
            }
        }

        private BodyType GetBodyType(dynamic obj)
        {
            var atypicalBodyTypes = new List<string>() { "Mask", "Hair", "Hand" };
            var regex = new Regex(@"\w+?_([a-zA-Z]+|[0-9]+).+");
            var fileType = regex.Match(this.cosmeticId).Groups[1].Value;
            var baseBodyType = Enum.Parse(typeof(BodyType), obj["Type"].Replace(obj["Type"][0], char.ToUpper(obj["Type"][0])));

            if(atypicalBodyTypes.Contains(fileType))
            {
                return (BodyType) Enum.Parse(typeof(BodyType), fileType); //??? why do I have to explicitly cast the result?...
            }
            else if(atypicalBodyTypeMapping.Keys.Contains(this.character.name))
            {
                //If we find Character in list, then we check if the base body type (value that'd have been getting normally) is mapped to other body part.
                //If not, continue with usual body part
                var mappedBodyTypes = atypicalBodyTypeMapping[this.character.name]; //.ToDictionary(x => x.Key, x => x.Value);
                return mappedBodyTypes.Keys.ToList().Contains(baseBodyType) ? mappedBodyTypes[baseBodyType] : baseBodyType;
            }
            else
            {
                return baseBodyType; //First letter Uppercase
            }
        }

        private void MapRift()
        {
            var riftRewardList = Rift.rifts.SelectMany(x => x.tiers).ToList();
            this.rift = Rift.rifts.SingleOrDefault(x => x.tiers.Select(t => t.rewardId).ToList().Contains(this.cosmeticId));
            this.riftTier = rift != null ? riftRewardList.SingleOrDefault(x => x.rewardId.Equals(this.cosmeticId)).tier : -1 ;
        }

        private void MapCharType(dynamic obj)
        {
            if(this.type != BodyType.Charm)
            {
                this.charType = Character.Killers.Values.Contains(this.character.dbdID) ? CharType.K : CharType.S;
            }
            else
            {
                switch(obj["Role"])
                {
                    case "None": this.charType = CharType.B; break;
                    case "Slasher": this.charType = CharType.K; break; //Killer
                    case "Camper": this.charType = CharType.S; break; //Survivor
                }
            }
                
        }

        private void MapChar()
        {
            if(Character.AllCharacters.Keys.Contains(this.character.dbdID)) {
                this.mappedChar = Character.AllCharacters.First(x => x.Key == this.character.dbdID);
            }
        }

        public int CompareTo(Cosmetic incomingCosmetic)
        {
            return this.cosmeticName.Replace(@"""", String.Empty)
                .Replace(@"The ", String.Empty)
                .CompareTo(incomingCosmetic.cosmeticName
                    .Replace(@"""", String.Empty)
                    .Replace(@"The ", String.Empty)
                );
        }
    }
}
