using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMRDataBridge
{
    public struct CycleOutput
    {
        public string State { get; set; }
        public string Type { get; set; }
        public string SrcID { get; set; }
        public string DstID { get; set; }
        public string SlotNum { get; set; }
        public string PktCount { get; set; }
        public string BER { get; set; }
        public string RSSI { get; set; }
        public string Notes { get; set; }

        public CycleOutput(CallCycleData callData, bool start)
        {
            if (start) 
            {
                State = "Active";
                PktCount = "";
                BER = "";
                RSSI = "";
                Notes = "";
            }
            else
            {
                State = "Idle";
                PktCount = callData.PacketCount.ToString();

                try{BER = callData.BER.Average().ToString("F3");}
                catch{BER = "0.000";}

                try{RSSI = callData.RSSI.Average().ToString("F1");}
                catch{RSSI = "0.0";}

                Notes = "TO "+ callData.TimeoutCount.ToString();
            }

            Type = DmrdPacket.GetPacketTypeCategoryText(callData.LastType);
            SrcID = callData.SrcId.ToString();
            DstID= callData.DstId.ToString();
            SlotNum = callData.SlotNo.ToString();

        }
    }
}
