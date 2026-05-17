using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMRDataBridge
{

    public enum CallState : byte
    {
        Waiting,
        Active,
        Timeout
    }

    public struct CallCycleData
    {
        public CallState CurrentCall { get; set; }
        public uint StreamId { get; set; }
        public uint SrcId { get; set; }
        public uint DstId { get; set; }
        public int SlotNo { get; set; }
        public PacketType LastType { get; set; }
        public int LastSeq { get; set; }
        public int PacketCount { get; set; }
        public List<int> BER { get; set; }
        public List<int> RSSI { get; set; }
        public int TimeoutCount { get; set; }

        public CallCycleData(DmrdPacket packet)
        {
            CurrentCall = CallState.Active;
            StreamId = packet.StreamID; 
            SrcId = packet.SrcId; 
            DstId = packet.DstId; 
            SlotNo = packet.SlotNo;
            LastType = packet.FrameType;
            LastSeq = packet.Seq;
            PacketCount = 1;
            BER = new List<int>(packet.BER);
            RSSI = new List<int>(packet.RSSI);
            TimeoutCount = 0;
        }

    }
}
