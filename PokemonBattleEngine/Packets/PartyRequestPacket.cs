﻿using Kermalis.EndianBinaryIO;
using System.Collections.ObjectModel;
using System.IO;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PBEPartyRequestPacket : IPBEPacket
    {
        public const ushort Code = 0x03;
        public ReadOnlyCollection<byte> Data { get; }

        public PBEPartyRequestPacket()
        {
            using (var ms = new MemoryStream())
            using (var w = new EndianBinaryWriter(ms, encoding: EncodingType.UTF16))
            {
                w.Write(Code);
                Data = new ReadOnlyCollection<byte>(ms.ToArray());
            }
        }
        internal PBEPartyRequestPacket(byte[] data)
        {
            Data = new ReadOnlyCollection<byte>(data);
        }
    }
}
