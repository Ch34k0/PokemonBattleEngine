﻿using Kermalis.EndianBinaryIO;
using Kermalis.PokemonBattleEngine.Battle;
using System;
using System.IO;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public static class PBEPacketProcessor
    {
        public static IPBEPacket CreatePacket(PBEBattle battle, byte[] data)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (battle.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(battle));
            }
            if (data.Length < 2)
            {
                throw new InvalidDataException();
            }
            using (var r = new EndianBinaryReader(new MemoryStream(data), encoding: EncodingType.UTF16))
            {
                ushort code = r.ReadUInt16();
                switch (code)
                {
                    case PBEResponsePacket.Code: return new PBEResponsePacket(data);
                    case PBEPlayerJoinedPacket.Code: return new PBEPlayerJoinedPacket(data, r);
                    case PBEMatchCancelledPacket.Code: return new PBEMatchCancelledPacket(data);
                    case PBEPartyRequestPacket.Code: return new PBEPartyRequestPacket(data);
                    case PBEPartyResponsePacket.Code: return new PBEPartyResponsePacket(data, r, battle);
                    case PBETeamPacket.Code: return new PBETeamPacket(data, r, battle);
                    case PBEPkmnSwitchInPacket.Code: return new PBEPkmnSwitchInPacket(data, r, battle);
                    case PBEActionsRequestPacket.Code: return new PBEActionsRequestPacket(data, r, battle);
                    case PBEActionsResponsePacket.Code: return new PBEActionsResponsePacket(data, r);
                    case PBEMoveUsedPacket.Code: return new PBEMoveUsedPacket(data, r, battle);
                    case PBEPkmnHPChangedPacket.Code: return new PBEPkmnHPChangedPacket(data, r, battle);
                    case PBEHazePacket.Code: return new PBEHazePacket(data);
                    case PBEPkmnSwitchOutPacket.Code: return new PBEPkmnSwitchOutPacket(data, r, battle);
                    case PBEMoveMissedPacket.Code: return new PBEMoveMissedPacket(data, r, battle);
                    case PBEPkmnFaintedPacket.Code: return new PBEPkmnFaintedPacket(data, r, battle);
                    case PBEMoveCritPacket.Code: return new PBEMoveCritPacket(data, r, battle);
                    case PBEPkmnStatChangedPacket.Code: return new PBEPkmnStatChangedPacket(data, r, battle);
                    case PBEStatus1Packet.Code: return new PBEStatus1Packet(data, r, battle);
                    case PBEStatus2Packet.Code: return new PBEStatus2Packet(data, r, battle);
                    case PBETeamStatusPacket.Code: return new PBETeamStatusPacket(data, r, battle);
                    case PBEWeatherPacket.Code: return new PBEWeatherPacket(data, r, battle);
                    case PBEMoveResultPacket.Code: return new PBEMoveResultPacket(data, r, battle);
                    case PBEItemPacket.Code: return new PBEItemPacket(data, r, battle);
                    case PBEMovePPChangedPacket.Code: return new PBEMovePPChangedPacket(data, r, battle);
                    case PBETransformPacket.Code: return new PBETransformPacket(data, r, battle);
                    case PBEAbilityPacket.Code: return new PBEAbilityPacket(data, r, battle);
                    case PBESpecialMessagePacket.Code: return new PBESpecialMessagePacket(data, r, battle);
                    case PBEBattleStatusPacket.Code: return new PBEBattleStatusPacket(data, r);
                    case PBEPsychUpPacket.Code: return new PBEPsychUpPacket(data, r, battle);
                    case PBESwitchInRequestPacket.Code: return new PBESwitchInRequestPacket(data, r, battle);
                    case PBESwitchInResponsePacket.Code: return new PBESwitchInResponsePacket(data, r);
                    case PBEIllusionPacket.Code: return new PBEIllusionPacket(data, r, battle);
                    case PBEWinnerPacket.Code: return new PBEWinnerPacket(data, r, battle);
                    case PBETurnBeganPacket.Code: return new PBETurnBeganPacket(data, r);
                    case PBEMoveLockPacket.Code: return new PBEMoveLockPacket(data, r, battle);
                    case PBEPkmnFormChangedPacket.Code: return new PBEPkmnFormChangedPacket(data, r, battle);
                    case PBEAutoCenterPacket.Code: return new PBEAutoCenterPacket(data, r, battle);
                    case PBETypeChangedPacket.Code: return new PBETypeChangedPacket(data, r, battle);
                    default: throw new InvalidDataException();
                }
            }
        }
    }
}
