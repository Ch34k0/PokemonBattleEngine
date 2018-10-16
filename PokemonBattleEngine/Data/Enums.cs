﻿using System;

namespace Kermalis.PokemonBattleEngine.Data
{
    public enum Gender : byte
    {
        Male = 0x00,
        M7F1 = 0x1F, // Male 7:1 Female
        M3F1 = 0x3F, // Male 3:1 Female
        M1F1 = 0x7F, // Male 1:1 Female
        M1F3 = 0xBF, // Male 1:3 Female
        M1F7 = 0xE1, // Male 1:7 Female // Does not exist before gen 6
        Female = 0xFE,
        Genderless = 0xFF
    }
    public enum Target : byte
    {
        All,
        AllyLeft,
        AllyMiddle,
        AllyRight,
        FoeLeft,
        FoeMiddle,
        FoeRight,
        Self
    }
    public enum Status : byte
    {
        None,
        Asleep,
        BadlyPoisoned,
        Burned,
        Frozen,
        Paralyzed,
        Poisoned
    }
    public enum Type : byte
    {
        Bug,
        Dark,
        Dragon,
        Electric,
        Fighting,
        Fire,
        Flying,
        Ghost,
        Grass,
        Ground,
        Ice,
        Normal,
        Poison,
        Psychic,
        Rock,
        Steel,
        Water
    }
    public enum Nature : byte
    {
        Adamant,
        Bashful,
        Bold,
        Brave,
        Calm,
        Careful,
        Docile,
        Gentle,
        Hardy,
        Hasty,
        Impish,
        Jolly,
        Lax,
        Loney,
        Mild,
        Modest,
        Naive,
        Naughty,
        Quiet,
        Quirky,
        Rash,
        Relaxed,
        Sassy,
        Serious,
        Timid,
        MAX
    }
    public enum Item : ushort
    {
        None,
        ChoiceBand,
        DeepSeaScale,
        DeepSeaTooth,
        Leftovers,
        LightBall,
        MetalPowder,
        SoulDew,
        ThickClub,
        MAX
    }
    public enum Ability : byte
    {
        None,
        BadDreams,
        BattleArmor,
        Blaze,
        Guts,
        HugePower,
        Hustle,
        Imposter,
        Levitate,
        LightningRod,
        Limber,
        MarvelScale,
        Minus,
        Overgrow,
        Plus,
        PurePower,
        Rattled,
        RockHead,
        SapSipper,
        ShellArmor,
        Static,
        Swarm,
        ThickFat,
        Torrent,
    }
    public enum Species : ushort
    {
        None,
        Pikachu = 25,
        Cubone = 104,
        Marowak,
        Ditto = 132,
        Azumarill = 184,
        Clamperl = 366,
        Latias = 380,
        Latios,
        Cresselia = 488,
        Darkrai = 491
    }
    public enum MoveCategory : byte
    {
        Status,
        Physical,
        Special
    }
    public enum PossibleTarget : byte // Used in MoveData
    {
        Any,
        AnySurrounding,
        Self
    }
    public enum MoveEffect : byte
    {
        Hit,
        Hit__MaybeFlinch,
        Hit__MaybeFreeze,
        Hit__MaybeLower_SPDEF_By1,
        Hit__MaybeParalyze,
        Lower_DEF_SPDEF_By1_Raise_ATK_SPATK_SPD_By2,
        Moonlight,
        Toxic,
        Transform,
    }
    [Flags]
    public enum MoveFlags : byte
    {
        None = 0,
        MakesContact = 1 << 0,
        AffectedByProtect = 1 << 1,
        AffectedByMagicCoat = 1 << 2,
        AffectedBySnatch = 1 << 3,
        AffectedByMirrorMove = 1 << 4
    }
    public enum Move : ushort
    {
        None,
        AquaJet,
        DarkPulse,
        DragonPulse,
        HydroPump,
        IceBeam,
        IcePunch,
        Moonlight,
        Psychic,
        Retaliate,
        Return,
        ShellSmash,
        Tackle,
        Thunder,
        Toxic,
        Transform,
        Waterfall,
    }
}
