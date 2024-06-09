using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMRDataBridge
{
    public enum PacketType : byte
    {
        DT_VOICE_PI_HEADER = 0,
        DT_VOICE_LC_HEADER = 1,
        DT_TERMINATOR_WITH_LC = 2,
        DT_CSBK = 3,
        UNKN4 = 4,
        UNKN5 = 5,
        DT_DATA_HEADER = 6,
        DT_RATE_12_DATA = 7,
        DT_RATE_34_DATA = 8,
        DT_IDLE = 9,
        DT_RATE_1_DATA = 10,
        UNKN11 = 11,
        UNKN12 = 12,
        UNKN13 = 13,
        UNKN14 = 14,
        UNKN15 = 15,
        DT_VOICE_SYNC = 16,
        DT_VOICE = 17,
        UNKN = 18
    }

    public struct DmrdPacket
    {
        public int PacketLength { get; }

        public byte RunningSeqNo { get; }

        public uint SrcId { get; }
        public uint DstId { get; }
        public uint RptrId { get; }

        public int SlotNo { get; }
        public bool CallType { get; } // False = Group / True = Other

        public int Seq { get; }
        public PacketType FrameType { get; }

        public uint StreamID { get; }
        public byte[] DmrData { get; }

        public byte BER { get; }
        public byte RSSI { get; }

        public DmrdPacket(byte[] packetData)
        {
            byte[] filler = { 0 };

            PacketLength = packetData.Length;

            RunningSeqNo = packetData[4];

            SrcId = BitConverter.ToUInt32(filler.Concat(packetData.Skip(5).Take(3)).Reverse().ToArray(), 0);
            DstId = BitConverter.ToUInt32(filler.Concat(packetData.Skip(8).Take(3)).Reverse().ToArray(), 0);
            RptrId = BitConverter.ToUInt32(packetData.Skip(11).Take(4).Reverse().ToArray(), 0);

            SlotNo = ((byte)(packetData[15] & (byte)0x80) != 0) ? 2 : 1 ;
            CallType = (byte)(packetData[15] & (byte)0x40) != 0;

            var FrmType = (byte)((packetData[15] & (byte)0x30) >> 4);
            var DataTypeVoiceSeq = (byte)(packetData[15] & (byte)0x0F);

            switch (FrmType)
            {
                case 0:
                    Seq = (int)DataTypeVoiceSeq;
                    FrameType = PacketType.DT_VOICE;
                    break;
                case 1:
                    Seq = (int)DataTypeVoiceSeq;
                    FrameType = PacketType.DT_VOICE_SYNC;
                    break;
                case 2:
                    Seq = 0;
                    FrameType = (PacketType)DataTypeVoiceSeq;
                    break;
                case 3:
                default:
                    Seq = 0;
                    FrameType = PacketType.UNKN;
                    break;
            }

            StreamID = BitConverter.ToUInt32(packetData.Skip(16).Take(4).Reverse().ToArray(), 0);

            DmrData = packetData.Skip(11).Take(33).ToArray();

            BER = packetData[53];
            RSSI = packetData[54];
        }


    }


}
