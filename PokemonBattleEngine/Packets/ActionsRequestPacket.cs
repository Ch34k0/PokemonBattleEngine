﻿using Kermalis.EndianBinaryIO;
using Kermalis.PokemonBattleEngine.Battle;
using Kermalis.PokemonBattleEngine.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PBEActionsRequestPacket : IPBEPacket
    {
        public const ushort Code = 0x07;
        public ReadOnlyCollection<byte> Data { get; }

        public PBETeam Team { get; }
        public ReadOnlyCollection<PBEFieldPosition> Pokemon { get; }

        internal PBEActionsRequestPacket(PBETeam team)
        {
            using (var ms = new MemoryStream())
            using (var w = new EndianBinaryWriter(ms, encoding: EncodingType.UTF16))
            {
                w.Write(Code);
                w.Write((Team = team).Id);
                byte count = (byte)(Pokemon = new ReadOnlyCollection<PBEFieldPosition>(Team.ActionsRequired.Select(p => p.FieldPosition).ToArray())).Count;
                w.Write(count);
                for (int i = 0; i < count; i++)
                {
                    w.Write(Pokemon[i]);
                }
                Data = new ReadOnlyCollection<byte>(ms.ToArray());
            }
        }
        internal PBEActionsRequestPacket(byte[] data, EndianBinaryReader r, PBEBattle battle)
        {
            Data = new ReadOnlyCollection<byte>(data);
            Team = battle.Teams[r.ReadByte()];
            var pkmn = new PBEFieldPosition[r.ReadByte()];
            for (int i = 0; i < pkmn.Length; i++)
            {
                pkmn[i] = r.ReadEnum<PBEFieldPosition>();
            }
            Pokemon = new ReadOnlyCollection<PBEFieldPosition>(pkmn);
        }
    }
}
