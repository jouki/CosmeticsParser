using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public class Rift
    {
        private static readonly string riftsJson = Encoding.UTF8.GetString(new WebClient().DownloadData("https://dbd-info.com/api/rifts"));
        private static Dictionary<string, dynamic> objectifiedRifts = JsonHelper.Deserialize(riftsJson);

        private static List<Rift> _rifts;
        public static List<Rift> rifts
        {
            get
            {
                if(_rifts == null)
                {
                    _rifts = PopulateRifts();
                }
                return _rifts;
            }
        }
            

        public int id;
        public string dbdName;
        public string name;
        //public string filename;
        public int requirement;
        public DateTime endDate;
        public List<RiftTier> tiers;

        public Rift(string key, dynamic value)
        {
            this.id = int.Parse(key.Replace("Tome", string.Empty));
            this.dbdName = key;
            this.name = Utils.RefactorName(value["Name"]);
            //this.filename = ((string) value["Banner"]).Split('/').Last(); //The Banner stopped being present in API (as of 16.12.2022)
            this.requirement = (int) value["Requirement"];
            this.endDate = value["EndDate"];
            this.tiers = GetRiftTierList(value["TierInfo"]);
        }

        private static List<Rift> PopulateRifts()
        {
            List<Rift> list = new List<Rift>();
            foreach(var (key, value) in objectifiedRifts.Select(x => (x.Key, x.Value)))
            {
                list.Add(new Rift(key, value));
            }
            return list;
        }

        private List<RiftTier> GetRiftTierList(List<dynamic> jsonTiers)
        {
            var list = new List<RiftTier>();

            foreach(var tierObj in jsonTiers)
            {
                int tier = (int) tierObj["tierId"];
                List<dynamic> freeRewards = tierObj.ContainsKey("free") ? tierObj["free"] : new List<dynamic>();
                List<dynamic> premiumReards = tierObj.ContainsKey("premium") ? tierObj["premium"] : new List<dynamic>();
                //jsonTiers.Select(x => (x["tierId"], x.ToDictionary(t => t.Key, t => t.Value)))

                freeRewards.ForEach(x => list.Add(new RiftTier(tier, RiftReward.Free, x)));
                premiumReards.ForEach(x => list.Add(new RiftTier(tier, RiftReward.Premium, x)));
            }

            return list;
        }

    }
}
