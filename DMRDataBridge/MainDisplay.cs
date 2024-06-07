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
                toolStripLabelPing.Text = "Ping";
                btnConnect.Enabled = false;
                tmrPing.Interval = Properties.Settings.Default.KeepAliveInterval;
                tmrPing.Enabled = true;
            }

            return _ep;
        }

        private void tmrPing_Tick(object sender, EventArgs e)
        {
            if (_connected)
            {
                bool pingGood = false;
                
                // ## Ping ##
                // send Ping
                byte[] pingCmd = Encoding.ASCII.GetBytes("RPTPING").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).ToArray();

                // Send Ping Msg
                _udpClient.Send(pingCmd, pingCmd.Length);

                // Read Response Body Ping
                var pingRespBody = _udpClient.Receive(ref _ep);

                if (pingRespBody.Length > 0)
                {
                    // Parse Response Body - Ping

                    var pingRespHeadNak = Encoding.ASCII.GetString(pingRespBody, 0, 6);
                    if ("MSTNAK" == pingRespHeadNak)
                    {
                        var pingRespStationIdNak = BitConverter.ToUInt32(pingRespBody.Skip(6).Reverse().ToArray(), 0);
                        if (Properties.Settings.Default.StationID == pingRespStationIdNak)
                        {
                            toolStripLabelPing.Text = "Ping: NAK";
                        }
                        else
                        {
                            toolStripLabelPing.Text = "Ping: NAK - Station Id Missmatch";
                        }
                    }
                    else
                    {
                        var pingRespHead = Encoding.ASCII.GetString(pingRespBody, 0, 7);
                        if ("MSTPONG" == pingRespHead)
                        {
                            var pingRespStationId = BitConverter.ToUInt32(pingRespBody.Skip(7).Reverse().ToArray(), 0);
                            if (Properties.Settings.Default.StationID == pingRespStationId)
                            {
                                pingGood = true;
                            }
                            else
                            {
                                toolStripLabelPing.Text = "Ping: Station Id Missmatch";
                            }
                        }
                        else
                        {
                            toolStripLabelPing.Text = "Ping: Unknown Response";
                        }
                    }

                }
                else
                {
                    toolStripLabelPing.Text = "Ping: No Response";
                }

                if (pingGood)
                {
                    if (_pingState)
                    {
                        toolStripLabelPing.Text = "Ping";
                    }
                    else
                    {
                        toolStripLabelPing.Text = "Pong";
                    }
                    _pingState = !_pingState;
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void MainDisplay_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_connected)
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            _connected = false;
            tmrPing.Enabled = false;
            _udpClient.Close();
            toolStripLabelConStatus.Text = "Disconnected";
            btnConnect.Enabled = true;
        }

    }
}
