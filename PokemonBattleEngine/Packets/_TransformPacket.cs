﻿using Kermalis.EndianBinaryIO;
using Kermalis.PokemonBattleEngine.Battle;
using Kermalis.PokemonBattleEngine.Data;
using System.Collections.ObjectModel;
using System.IO;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PBETransformPacket : IPBEPacket
    {
        public const ushort Code = 0x18;
        public ReadOnlyCollection<byte> Data { get; }

        public PBEFieldPosition User { get; }
        public PBETeam UserTeam { get; }
        public PBEFieldPosition Target { get; }
        public PBETeam TargetTeam { get; }
        public ushort TargetAttack { get; }
        public ushort TargetDefense { get; }
        public ushort TargetSpAttack { get; }
        public ushort TargetSpDefense { get; }
        public ushort TargetSpeed { get; }
        public sbyte TargetAttackChange { get; }
        public sbyte TargetDefenseChange { get; }
        public sbyte TargetSpAttackChange { get; }
        public sbyte TargetSpDefenseChange { get; }
        public sbyte TargetSpeedChange { get; }
        public sbyte TargetAccuracyChange { get; }
        public sbyte TargetEvasionChange { get; }
        public PBEAbility TargetAbility { get; }
        public PBESpecies TargetSpecies { get; }
        public PBEType TargetType1 { get; }
        public PBEType TargetType2 { get; }
        public double TargetWeight { get; }
        public ReadOnlyCollection<PBEMove> TargetMoves { get; }

        internal PBETransformPacket(PBEPokemon user, PBEPokemon target)
        {
            using (var ms = new MemoryStream())
            using (var w = new EndianBinaryWriter(ms, encoding: EncodingType.UTF16))
            {
                w.Write(Code);
                w.Write(User = user.FieldPosition);
                w.Write((UserTeam = user.Team).Id);
                w.Write(Target = target.FieldPosition);
                w.Write((TargetTeam = target.Team).Id);
                w.Write(TargetAttack = target.Attack);
                w.Write(TargetDefense = target.Defense);
                w.Write(TargetSpAttack = target.SpAttack);
                w.Write(TargetSpDefense = target.SpDefense);
                w.Write(TargetSpeed = target.Speed);
                w.Write(TargetAttackChange = target.AttackChange);
                w.Write(TargetDefenseChange = target.DefenseChange);
                w.Write(TargetSpAttackChange = target.SpAttackChange);
                w.Write(TargetSpDefenseChange = target.SpDefenseChange);
                w.Write(TargetSpeedChange = target.SpeedChange);
                w.Write(TargetAccuracyChange = target.AccuracyChange);
                w.Write(TargetEvasionChange = target.EvasionChange);
                w.Write(TargetAbility = target.Ability);
                w.Write(TargetSpecies = target.Species);
                w.Write(TargetType1 = target.Type1);
                w.Write(TargetType2 = target.Type2);
                w.Write(TargetWeight = target.Weight);
                TargetMoves = target.Moves.ForTransformPacket();
                for (int i = 0; i < TargetMoves.Count; i++)
                {
                    w.Write(TargetMoves[i]);
                }
                Data = new ReadOnlyCollection<byte>(ms.ToArray());
            }
        }
        internal PBETransformPacket(byte[] data, EndianBinaryReader r, PBEBattle battle)
        {
            Data = new ReadOnlyCollection<byte>(data);
            User = r.ReadEnum<PBEFieldPosition>();
            UserTeam = battle.Teams[r.ReadByte()];
            Target = r.ReadEnum<PBEFieldPosition>();
            TargetTeam = battle.Teams[r.ReadByte()];
            TargetAttack = r.ReadUInt16();
            TargetDefense = r.ReadUInt16();
            TargetSpAttack = r.ReadUInt16();
            TargetSpDefense = r.ReadUInt16();
            TargetSpeed = r.ReadUInt16();
            TargetAttackChange = r.ReadSByte();
            TargetDefenseChange = r.ReadSByte();
            TargetSpAttackChange = r.ReadSByte();
            TargetSpDefenseChange = r.ReadSByte();
            TargetSpeedChange = r.ReadSByte();
            TargetAccuracyChange = r.ReadSByte();
            TargetEvasionChange = r.ReadSByte();
            TargetAbility = r.ReadEnum<PBEAbility>();
            TargetSpecies = r.ReadEnum<PBESpecies>();
            TargetType1 = r.ReadEnum<PBEType>();
            TargetType2 = r.ReadEnum<PBEType>();
            TargetWeight = r.ReadDouble();
            var moves = new PBEMove[battle.Settings.NumMoves];
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i] = r.ReadEnum<PBEMove>();
            }
            TargetMoves = new ReadOnlyCollection<PBEMove>(moves);
        }
    }
}
