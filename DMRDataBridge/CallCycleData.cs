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

    }
}
