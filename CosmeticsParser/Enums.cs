using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public enum BodyType
    {
        Outfit = 1,
        SurvivorHead = 2,
        KillerHead = 3,
        SurvivorTorso = 4,
        KillerBody = 5,
        SurvivorLegs = 6,
        KillerWeapon = 7,
        Charm = 8,

        //Atypic types
        Mask = 9,
        Hair = 10,
        Arm = 11,
        Hand = 12,
        UpperBody = 13,
        KillerLegs = 14,
        Badge = 15,
        Banner = 16,
    }
    public enum Rarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        VeryRare = 4,
        UltraRare = 5,
        Teachable = 6,
        Legendary = 7,
        SpecialEvent = 8,
        Artifact = 9
    }

    public enum Currency
    {
        Shards,
        Cells,
        HalloweenEventCurrency,
        WinterEventCurrency,
        AnniversaryEventCurrency
    }

    public enum CharType
    {
        S,  //Survivors
        K,  //Killers
        B   //Both
    }

    public enum RiftReward
    {
        Free,
        Premium
    }

    public enum Module
    {
        Datatable,
        Datatable_SoS,
        Datatable_Icons,
        Datatable_Loadout,
        Datatable_Perks,
        Datatable_Offerings,
        Datatable_Cosmetics,

        Cosmetics,
        DLCs,
        Killers,
        Maps,
        Offerings,
        Perks,
        SoS,
        Survivors,
        Various,

        Extensions,
        Languages,
        MathOps,
        PerkImage,
        Strings,
        Utils
    }


}
