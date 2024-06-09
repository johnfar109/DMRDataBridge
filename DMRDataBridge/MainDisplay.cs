using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMRDataBridge
{

    public partial class MainDisplay : Form
    {
        private UdpClient _udpClient;
        private IPEndPoint _ep;

        private uint _phase1RespSalt;

        private bool _connected = false;

        private bool _pingState = false;
        private bool _pingActive = false;

        IAsyncResult ar_ = null;

        private int packetCount = 0;

        public MainDisplay()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //setup UDP Socket
            _udpClient = new UdpClient();
            IPEndPoint _ep = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.Host), Properties.Settings.Default.HostPort); // endpoint where server is listening


            _ep = Connect(_ep);

        }

        private IPEndPoint Connect(IPEndPoint _ep)
        {
            // open the port 
            _udpClient.Connect(_ep);

            bool doPhase2 = false;
            bool doPhase3 = false;
            bool loginGood = false;

            // ## Phase 1 ##
            toolStripLabelConStatus.Text = "Login Phase 1";
            // Form and send Login Phase 1 Msg
            byte[] phase1Cmd = Encoding.ASCII.GetBytes("RPTL").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).ToArray();

            // Send Phase 1 Msg
            _udpClient.Send(phase1Cmd, phase1Cmd.Length);

            // Read Response Body Login Phase 1
            var phase1RespBody = _udpClient.Receive(ref _ep);

            if (phase1RespBody.Length > 0)
            {
                // Parse Response Body - Login Phase 1
                var phase1RespHead = Encoding.ASCII.GetString(phase1RespBody, 0, 6);
                if ("RPTACK" == phase1RespHead)
                {
                    _phase1RespSalt = BitConverter.ToUInt32(phase1RespBody.Skip(6).Reverse().ToArray(), 0);
                    doPhase2 = true;
                }
                else if ("RPTNAK" == phase1RespHead)
                {
                    toolStripLabelConStatus.Text = "Login Phase 1: Rejected";
                }
                else
                {
                    toolStripLabelConStatus.Text = "Login Phase 1: Unknown Response";
                }
            }
            else
            {
                toolStripLabelConStatus.Text = "Login Phase 1: No Response";
            }

            if (doPhase2)
            {
                // ## Phase 2 ##
                toolStripLabelConStatus.Text = "Login Phase 2";
                // Form and send Login Phase 2 Msg
                byte[] phase2Secret = BitConverter.GetBytes(_phase1RespSalt).Reverse().ToArray().Concat(Encoding.ASCII.GetBytes(Properties.Settings.Default.Password)).ToArray();
                SHA256Managed hashstring = new SHA256Managed();
                byte[] secretHash = hashstring.ComputeHash(phase2Secret);

                byte[] phase2Cmd = Encoding.ASCII.GetBytes("RPTK").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).Concat(secretHash).ToArray();

                // Send Phase 2 Msg
                _udpClient.Send(phase2Cmd, phase2Cmd.Length);

                // Read Response Body Login Phase 2
                var phase2RespBody = _udpClient.Receive(ref _ep);

                if (phase2RespBody.Length > 0)
                {
                    // Parse Response Body - Login Phase 2
                    var phase2RespHead = Encoding.ASCII.GetString(phase2RespBody, 0, 6);
                    if ("RPTACK" == phase2RespHead)
                    {
                        var phase2RespStationId = BitConverter.ToUInt32(phase2RespBody.Skip(6).Reverse().ToArray(), 0);
                        if (Properties.Settings.Default.StationID == phase2RespStationId)
                        {
                            doPhase3 = true;
                        }
                        else
                        {
                            toolStripLabelConStatus.Text = "Login Phase 2: Station Id Missmatch";
                        }
                    }
                    else if ("RPTNAK" == phase2RespHead)
                    {
                        toolStripLabelConStatus.Text = "Login Phase 2: Rejected";
                    }
                    else
                    {
                        toolStripLabelConStatus.Text = "Login Phase 2: Unknown Response";
                    }
                }
                else
                {
                    toolStripLabelConStatus.Text = "Login Phase 2: No Response";
                }
            }

            if (doPhase3)
            {
                // ## Phase 3 ##
                toolStripLabelConStatus.Text = "Login Phase 3";
                // send Login phase 3 (Conig Data)
                byte[] phase3Cmd = Encoding.ASCII.GetBytes("RPTC").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).Concat(Encoding.ASCII.GetBytes(Properties.Settings.Default.Callsign)).ToArray();
                // TODO Pack the rest of the station Config Data

                // Send Phase 3 Msg
                _udpClient.Send(phase3Cmd, phase3Cmd.Length);

                // Read Response Body Login Phase 3
                var phase3RespBody = _udpClient.Receive(ref _ep);

                if (phase3RespBody.Length > 0)
                {
                    // Parse Response Body - Login Phase 3
                    var phase3RespHead = Encoding.ASCII.GetString(phase3RespBody, 0, 6);
                    if ("RPTACK" == phase3RespHead)
                    {
                        var phase3RespStationId = BitConverter.ToUInt32(phase3RespBody.Skip(6).Reverse().ToArray(), 0);
                        if (Properties.Settings.Default.StationID == phase3RespStationId)
                        {
                            loginGood = true;
                        }
                        else
                        {
                            toolStripLabelConStatus.Text = "Login Phase 3: Station Id Missmatch";
                        }
                    }
                    else if ("RPTNAK" == phase3RespHead)
                    {
                        toolStripLabelConStatus.Text = "Login Phase 3: Rejected";
                    }
                    else
                    {
                        toolStripLabelConStatus.Text = "Login Phase 3: Unknown Response";
                    }

                }
                else
                {
                    toolStripLabelConStatus.Text = "Login Phase 3: No Response";
                }
            }

            if (loginGood)
            {
                _connected = true;
                toolStripLabelConStatus.Text = "Connected";
                toolStripLabelStatus.Text = "Ping";
                btnConnect.Enabled = false;
                tmrPing.Interval = Properties.Settings.Default.KeepAliveInterval;
                tmrPing.Enabled = true;
                _pingActive = false;

                StartListening();
            }

            return _ep;
        }

        private void tmrPing_Tick(object sender, EventArgs e)
        {
            if (_connected)
            {
                if (!_pingActive)
                {
                    // ## Ping ##
                    // send Ping
                    byte[] pingCmd = Encoding.ASCII.GetBytes("RPTPING").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).ToArray();

                    // Send Ping Msg
                    _udpClient.Send(pingCmd, pingCmd.Length);
                    _pingActive = true;
                }
                else
                {
                    //The last Ping Never got a Pong
                    //Shut it down
                    Disconnect();
                }
            }
        }

        private void MainDisplay_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_connected)
            {
                // send close
                byte[] closeCmd = Encoding.ASCII.GetBytes("RPTCL").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).ToArray();
                
                // Send close Msg
                _udpClient.Send(closeCmd, closeCmd.Length);

                Disconnect();
            }
        }

        private void StartListening()
        {
            if (_connected)
            {
                ar_ = _udpClient.BeginReceive(Receive, new object());
            }
        }

        private void Receive(IAsyncResult ar)
        {
            if (_connected)
            {
                bool packetGood = false;
                byte[] respBody = _udpClient.EndReceive(ar, ref _ep);

                if (respBody.Length > 0)
                {
                    var packetType = Encoding.ASCII.GetString(respBody, 0, 4);
                    switch (packetType)
                    {
                        case "MSTP":
                            //Ping
                            // Parse Response Body - Ping
                            var pingRespHead = Encoding.ASCII.GetString(respBody, 0, 7);
                            if ("MSTPONG" == pingRespHead)
                            {
                                var pingRespStationId = BitConverter.ToUInt32(respBody.Skip(7).Reverse().ToArray(), 0);
                                if (Properties.Settings.Default.StationID == pingRespStationId)
                                {
                                    packetGood = true;
                                    _pingActive = false;

                                    if (_pingState)
                                    {
                                        toolStripLabelStatus.Text = "Ping";
                                    }
                                    else
                                    {
                                        toolStripLabelStatus.Text = "Pong";
                                    }
                                    _pingState = !_pingState;

                                }
                                else
                                {
                                    toolStripLabelStatus.Text = "Ping: Station Id Missmatch";
                                }
                            }
                            else
                            {
                                toolStripLabelStatus.Text = "Error: Unknown 'MSTP' Response";
                            }
                            break;
                        case "MSTN":
                            // check for Master NAK
                            var respHeadNak = Encoding.ASCII.GetString(respBody, 0, 6);
                            if ("MSTNAK" == respHeadNak)
                            {
                                var respStationIdNak = BitConverter.ToUInt32(respBody.Skip(6).Reverse().ToArray(), 0);
                                if (Properties.Settings.Default.StationID == respStationIdNak)
                                {
                                    toolStripLabelStatus.Text = "Error: NAK";
                                }
                                else
                                {
                                    toolStripLabelStatus.Text = "Error: NAK - Station Id Missmatch";
                                }
                            }
                            else
                            {
                                toolStripLabelStatus.Text = "Error: Unknown 'MSTN' Response";
                            }
                            break;
                        case "DMRD":
                            // DMR Daata Packets
                            packetGood = true;

                            var packet = new DmrdPacket(respBody);

                            // DEBUG - Debug Display Packet Count
                            packetCount++;
                            DisplayPacketCount();
                            
                            break;
                        case "MSTC":
                            // Check to See if a close command form Master
                            var closeRespHead = Encoding.ASCII.GetString(respBody, 0, 5);
                            if ("MSTCL"== closeRespHead)
                            {
                                // sutdown the connection
                                packetGood = false;
                                toolStripLabelStatus.Text = "Master: Close";
                            }
                            else
                            {
                                toolStripLabelStatus.Text = "Error: Unknown 'MSTC' Response";
                            }
                            break;
                        default:
                                toolStripLabelStatus.Text = "Error: Unknown Packet Type " + packetType;
                            break;
                    } 
                }
                else
                {
                    toolStripLabelStatus.Text = "Error: Empty Packet";
                }

                if (packetGood)
                {
                    // TODO check or anything else?
                }
                else
                {
                    Disconnect();
                }


                StartListening();
            }
        }

        private void Disconnect()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(Disconnect));
            }
            else
            {
                _connected = false;
                tmrPing.Enabled = false;
                _udpClient.Close();
                toolStripLabelConStatus.Text = "Disconnected";
                btnConnect.Enabled = true;
            }
        }

        private void DisplayPacketCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DisplayPacketCount));
            }
            else
            {
                labelPacketCount.Text = packetCount.ToString();
            }
        }

    }
}
