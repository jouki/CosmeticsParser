namespace CosmeticsParser
{
    public class RiftTier
    {
        public int tier;
        public RiftReward rewardType;
        public string rewardId;
        public int amount;
        public string type;

        public RiftTier(int tier, RiftReward rewardType, dynamic rewardObj)
        {
            this.tier = tier;
            this.rewardType = rewardType;
            this.rewardId = rewardObj["Id"];
            this.amount = (int) rewardObj["Amount"];
            this.type = rewardObj["Type"];
        }
    }
}