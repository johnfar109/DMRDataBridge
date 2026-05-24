using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt.Messages;
using static System.Windows.Forms.AxHost;

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

        private string _toolStripLabelStatus = string.Empty;

        private int packetCount = 0;
        private int packetProcCount = 0;
        private DmrdPacket _debugPacket;
        private CycleOutput _cycleOutput;

        private Channel<DmrdPacket> dmrdPacketQueue = Channel.CreateUnbounded<DmrdPacket>();

        private uPLibrary.Networking.M2Mqtt.MqttClient _mqttRadioMonitorClient;

        private bool _mqttConnected = false;

        //for Cleanup
        private bool _Closing = false;

        public MainDisplay()
        {
            InitializeComponent();
        }

        private void MainDisplay_Load(object sender, EventArgs e)
        {
            //Start the loop to process Packets
            Task consumer = ConsumeAsync(dmrdPacketQueue.Reader);
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
            // Set the timeouts
            _udpClient.Client.SendTimeout = Properties.Settings.Default.UdpTimeout;
            _udpClient.Client.ReceiveTimeout = Properties.Settings.Default.UdpTimeout;

            bool doPhase2 = false;
            bool doPhase3 = false;
            bool loginGood = false;

            // ## Phase 1 ##
            toolStripLabelHbLinkConStatus.Text = "Login Phase 1";
            // Form and send Login Phase 1 Msg
            byte[] phase1Cmd = Encoding.ASCII.GetBytes("RPTL").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).ToArray();

            try
            {
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
                        toolStripLabelHbLinkConStatus.Text = "Login Phase 1: Rejected";
                    }
                    else
                    {
                        toolStripLabelHbLinkConStatus.Text = "Login Phase 1: Unknown Response";
                    }
                }
                else
                {
                    toolStripLabelHbLinkConStatus.Text = "Login Phase 1: No Response";
                }
            }
            catch (SocketException)
            {
                toolStripLabelHbLinkConStatus.Text = "Login Phase 1: Timeout - No Response";
            }


            if (doPhase2)
            {
                // ## Phase 2 ##
                toolStripLabelHbLinkConStatus.Text = "Login Phase 2";
                // Form and send Login Phase 2 Msg
                byte[] phase2Secret = BitConverter.GetBytes(_phase1RespSalt).Reverse().ToArray().Concat(Encoding.ASCII.GetBytes(Properties.Settings.Default.Password)).ToArray();
                SHA256Managed hashstring = new SHA256Managed();
                byte[] secretHash = hashstring.ComputeHash(phase2Secret);

                byte[] phase2Cmd = Encoding.ASCII.GetBytes("RPTK").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).Concat(secretHash).ToArray();

                try
                {
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
                                toolStripLabelHbLinkConStatus.Text = "Login Phase 2: Station Id Missmatch";
                            }
                        }
                        else if ("RPTNAK" == phase2RespHead)
                        {
                            toolStripLabelHbLinkConStatus.Text = "Login Phase 2: Rejected";
                        }
                        else
                        {
                            toolStripLabelHbLinkConStatus.Text = "Login Phase 2: Unknown Response";
                        }
                    }
                    else
                    {
                        toolStripLabelHbLinkConStatus.Text = "Login Phase 2: No Response";
                    }
                }
                catch (SocketException)
                {
                    toolStripLabelHbLinkConStatus.Text = "Login Phase 2: Timeout - No Response";
                }
            }

            if (doPhase3)
            {
                // ## Phase 3 ##
                toolStripLabelHbLinkConStatus.Text = "Login Phase 3";
                // send Login phase 3 (Conig Data)
                byte[] phase3Cmd = Encoding.ASCII.GetBytes("RPTC").Concat(BitConverter.GetBytes(Properties.Settings.Default.StationID).Reverse().ToArray()).Concat(Encoding.ASCII.GetBytes(Properties.Settings.Default.Callsign)).ToArray();
                // TODO Pack the rest of the station Config Data

                try
                {
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
                                toolStripLabelHbLinkConStatus.Text = "Login Phase 3: Station Id Missmatch";
                            }
                        }
                        else if ("RPTNAK" == phase3RespHead)
                        {
                            toolStripLabelHbLinkConStatus.Text = "Login Phase 3: Rejected";
                        }
                        else
                        {
                            toolStripLabelHbLinkConStatus.Text = "Login Phase 3: Unknown Response";
                        }

                    }
                    else
                    {
                        toolStripLabelHbLinkConStatus.Text = "Login Phase 3: No Response";
                    }
                }
                catch (SocketException)
                {
                    toolStripLabelHbLinkConStatus.Text = "Login Phase 3: Timeout - No Response";
                }
            }

            if (loginGood)
            {
                _connected = true;
                toolStripLabelHbLinkConStatus.Text = "Connected";
                _toolStripLabelStatus= "Ping";
                DisplayToolStripLabelStatus();
                btnHblinkConnect.Enabled = false;
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

            // handle closing of Radio Monitor connections
            _Closing = true;

            if (_mqttRadioMonitorClient != null && _mqttRadioMonitorClient.IsConnected)
            {
                _mqttRadioMonitorClient.Disconnect();
            }

            dmrdPacketQueue.Writer.Complete();
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
                                        _toolStripLabelStatus = "Ping";
                                        DisplayToolStripLabelStatus();
                                    }
                                    else
                                    {
                                        _toolStripLabelStatus= "Pong";
                                        DisplayToolStripLabelStatus();
                                    }
                                    _pingState = !_pingState;

                                }
                                else
                                {
                                    _toolStripLabelStatus = "Ping: Station Id Missmatch";
                                    DisplayToolStripLabelStatus();
                                }
                            }
                            else
                            {
                                _toolStripLabelStatus = "Error: Unknown 'MSTP' Response";
                                DisplayToolStripLabelStatus();
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
                                    _toolStripLabelStatus = "Error: NAK";
                                    DisplayToolStripLabelStatus();
                                }
                                else
                                {
                                    _toolStripLabelStatus = "Error: NAK - Station Id Missmatch";
                                    DisplayToolStripLabelStatus();
                                }
                            }
                            else
                            {
                                _toolStripLabelStatus = "Error: Unknown 'MSTN' Response";
                                DisplayToolStripLabelStatus();
                            }
                            break;
                        case "DMRD":
                            // DMR Daata Packets
                            packetGood = true;

                            var packet = new DmrdPacket(respBody);

                            // DEBUG - Debug Display Packet Count
                            packetCount++;
                            DisplayPacketCount();

                            //Send the DMRD packet over to be processed
                            dmrdPacketQueue.Writer.WriteAsync(packet);


                            break;
                        case "MSTC":
                            // Check to See if a close command form Master
                            var closeRespHead = Encoding.ASCII.GetString(respBody, 0, 5);
                            if ("MSTCL"== closeRespHead)
                            {
                                // sutdown the connection
                                packetGood = false;
                                _toolStripLabelStatus = "Master: Close";
                                DisplayToolStripLabelStatus();
                            }
                            else
                            {
                                _toolStripLabelStatus = "Error: Unknown 'MSTC' Response";
                                DisplayToolStripLabelStatus();
                            }
                            break;
                        default:
                                _toolStripLabelStatus = "Error: Unknown Packet Type " + packetType;
                                DisplayToolStripLabelStatus();
                            break;
                    } 
                }
                else
                {
                    _toolStripLabelStatus = "Error: Empty Packet";
                    DisplayToolStripLabelStatus();
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
                toolStripLabelHbLinkConStatus.Text = "Disconnected";
                btnHblinkConnect.Enabled = true;
            }
        }

        private async Task ConsumeAsync(ChannelReader<DmrdPacket> reader)
        {
            DmrdPacket packet;
    
            Dictionary<uint, CallCycleData> calls= new Dictionary<uint, CallCycleData>();

            CallCycleData stationCall;

            while (true)
            {
                try
                {
                    //Look to see if we have a packet waiting 
                    if (reader.TryRead(out packet))
                    {
                        packetProcCount++;
                        DisplayProcPacketCount();

                        // Spit out some debug
                        _debugPacket = packet;
                        DisplayPacketDebug();

                        // Check for a call record for this station
                        if (calls.TryGetValue(packet.RptrId, out stationCall))
                        {
                            //we have call record for this station(Rptr) (stationCall)
                            switch (calls[packet.RptrId].CurrentCall)
                            {
                                case CallState.Active:
                                    {
                                        // call is allready running 
                                        if (stationCall.StreamId == packet.StreamID)
                                        {
                                            //continue of exisiting call

                                            stationCall.PacketCount++;
                                            stationCall.BER.Add(packet.BER);
                                            stationCall.RSSI.Add(packet.RSSI);
                                            PacketType LastTypeEval = stationCall.LastType;
                                            stationCall.LastType = packet.FrameType;
                                            stationCall.LastSeq = packet.Seq;
                                            stationCall.TimeoutCount = 0;
                                            stationCall.Packets.Add(packet);

                                            //try and figure out if this is the end of the transmition
                                            if ((PacketType.DT_VOICE == LastTypeEval) && (packet.FrameType == PacketType.DT_TERMINATOR_WITH_LC) ||
                                                (PacketType.DT_DATA_HEADER == LastTypeEval) && (packet.FrameType == PacketType.DT_RATE_12_DATA) ||
                                                (packet.FrameType == LastTypeEval) && (packet.FrameType == PacketType.DT_TERMINATOR_WITH_LC) )
                                            {
                                                stationCall.CurrentCall = CallState.Waiting;
                                                RegisterCallAction(stationCall, false);
                                            }

                                        }
                                        else
                                        {
                                            // Start of new call, StreamIDs did not match
                                            stationCall = new CallCycleData(packet);
                                            RegisterCallAction(stationCall,true);
                                        }
                                        break;
                                    }
                                case CallState.Waiting:
                                default:
                                    {
                                        // Start of new call from Idle
                                        stationCall = new CallCycleData(packet);
                                        RegisterCallAction(stationCall, true);
                                        break;
                                    }
                            }

                            // Store back the call record for this station
                            calls[packet.RptrId]= stationCall;

                        }
                        else
                        {
                            // we are starting a new call record for this station
                            // Start of new call
                            stationCall = new CallCycleData(packet);
                            RegisterCallAction(stationCall, true);

                            // Store a new record for this station
                            calls.Add(packet.RptrId, stationCall);
                        }


                    }
                    else
                    {
                        //Bump all the timeout counters while we wait for packets
                        foreach (var key in calls.Keys.ToList())
                        {
                            CallCycleData _tmp = calls[key];
                            //if we have a call that is active bump up the timeout counters and check if we have timed out
                            if (_tmp.CurrentCall == CallState.Active)
                            {
                                _tmp.TimeoutCount++;

                                if ((_tmp.TimeoutCount >= Properties.Settings.Default.VoiceTimeoutCount && DmrdPacket.GetPacketTypeIsVoice(_tmp.LastType)) ||
                                    (_tmp.TimeoutCount >= Properties.Settings.Default.DataTimeoutCount && DmrdPacket.GetPacketTypeIsData(_tmp.LastType)))
                                {
                                    // We have tri[ped a timeout waiting for more ... or the right Termination packets, call it off
                                    _tmp.CurrentCall = CallState.Waiting;
                                    RegisterCallAction(_tmp, false);
                                }
                                //Save the update
                                calls[key] = _tmp;
                            }

                        }
                        //wait 10ms And do it again
                        await Task.Delay(10);
                    }
                }
                catch (Exception ex)
                {
                    //Trap for tracking down Exceptions
                    var exMsg = ex.Message;
                }
            }
        }

        private void RegisterCallAction(CallCycleData call, bool start)
        {
            // Do things with the call that is starting or ending 
            _cycleOutput = new CycleOutput(call, start);
            DisplayCallInList();

            if (_mqttConnected && Properties.Settings.Default.PublishMQTT)
            {
                string _topic = Properties.Settings.Default.OutputDisplayTopic;
                string _json = JsonConvert.SerializeObject(_cycleOutput);
                byte[] _msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_cycleOutput));
                _mqttRadioMonitorClient.Publish(_topic, _msg, 0, !start);
            }
        }

        // ** Display helpers **

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

        private void DisplayToolStripLabelStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DisplayToolStripLabelStatus));
            }
            else
            {
                toolStripLabelStatus.Text = _toolStripLabelStatus;
            }
        }

        private void DisplayProcPacketCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DisplayProcPacketCount));
            }
            else
            {
                labelProcPacketCount.Text = packetProcCount.ToString();
            }
        }

        private void DisplayPacketDebug()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DisplayPacketDebug));
            }
            else
            {
                lblPacket.Text = _debugPacket.FrameType.ToString();
                lblRptrId.Text = _debugPacket.RptrId.ToString();
            }
        }

        private void DisplayCallInList()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DisplayCallInList));
            }
            else
            {
                listBoxCalls.Items.Add(JsonConvert.SerializeObject(_cycleOutput));
            }
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            listBoxCalls.Items.Clear();
        }

        private void btnClrCounts_Click(object sender, EventArgs e)
        {
            packetCount = 0;
            packetProcCount = 0;
            labelPacketCount.Text = packetCount.ToString();
            labelProcPacketCount.Text = packetProcCount.ToString();
        }

        private void btnMqttConnect_Click(object sender, EventArgs e)
        {
            RadioMonitorInit();

            btnMqttConnect.Enabled = false;
            tmrMqttCon.Enabled = true;
            CheckMqttStatus();
        }

        private void RadioMonitorInit()
        {
            // create client instance 
            _mqttRadioMonitorClient = new uPLibrary.Networking.M2Mqtt.MqttClient(Properties.Settings.Default.MqttServer);

            _mqttRadioMonitorClient.ConnectionClosed += _client_MqttRadioMonitorConnectionClosed;

            // register to message received 
            //_mqttRadioMonitorClient.MqttMsgPublishReceived += _client_MqttRadioMonitorMsgPublishReceived;

            RadioMonitorConnectAndSubscribe();
        }

        // ** MQTT Connection Managment **
        private void RadioMonitorConnectAndSubscribe()
        {
            string clientId = "Radio Monitor Listen";

            try
            {
                _mqttRadioMonitorClient.Connect(clientId, string.Empty, string.Empty, true, 15);
            }
            catch (uPLibrary.Networking.M2Mqtt.Exceptions.MqttConnectionException cex)
            {
                //Nothing, try again
                RadioMonitorHandleDisconnect();
            }
            catch (uPLibrary.Networking.M2Mqtt.Exceptions.MqttCommunicationException cex)
            {
                //Nothing, try again
                RadioMonitorHandleDisconnect();
            }

            // subscribe to the topic  
            //string[] topics = new string[] { Properties.Settings.Default.RadioMonitorChanTopic };
            //byte[] qosLevels = new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE };

            //_mqttRadioMonitorClient.Subscribe(topics, qosLevels);

            _mqttConnected = true;
            RadioMonitorShowConnected();

        }

        /*private void _client_MqttRadioMonitorMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if ((e.Topic == Properties.Settings.Default.RadioMonitorChanTopic))
            {
                string payload = System.Text.UTF8Encoding.UTF8.GetString(e.Message);
                ReadRadioMonitorMsg(payload, e.Topic);
            }
        }*/

        private void _client_MqttRadioMonitorConnectionClosed(object sender, EventArgs e)
        {
            if (!_Closing)
            {
                RadioMonitorHandleDisconnect();
            }
        }

        private void RadioMonitorHandleDisconnect()
        {
            _mqttConnected = false;
            RadioMonitorShowDisconnected();
            System.Threading.Thread.Sleep(1);

            if (!_Closing)
                RadioMonitorConnectAndSubscribe();
        }

        private void tmrMqttCon_Tick(object sender, EventArgs e)
        {
            CheckMqttStatus();
        }

        private void CheckMqttStatus()
        {
            try
            {
                if (_mqttRadioMonitorClient != null && _mqttRadioMonitorClient.IsConnected)
                {
                    //toolStripLabelMqttConStatus.Text = "MQTT Connected";
                    _mqttConnected = true;
                    RadioMonitorShowConnected();
                }
                else if (!_mqttRadioMonitorClient.IsConnected)
                {
                    //toolStripLabelMqttConStatus.Text = "MQTT Disconnected";
                    _mqttConnected = false;
                    RadioMonitorShowDisconnected();
                }
            }
            catch
            {
                toolStripLabelMqttConStatus.Text = "MQTT Failure";
            }

        }

        private void RadioMonitorShowConnected()
        {
            if (!_Closing)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(RadioMonitorShowConnected));
                }
                else
                {
                    this.lblServerConnStatus.Text = "Connected";
                    this.lblServerConnStatus.BackColor = Color.Green;
                    toolStripLabelMqttConStatus.Text = "MQTT Connected";
                }
            }
        }

        private void RadioMonitorShowDisconnected()
        {
            if (!_Closing)
            {
                if (this.InvokeRequired)
                {
                    try
                    {
                        this.Invoke(new MethodInvoker(RadioMonitorShowDisconnected));
                    }
                    catch { }
                }
                else
                {
                    this.lblServerConnStatus.Text = "DISCONNECTED";
                    this.lblServerConnStatus.BackColor = Color.Red;
                    toolStripLabelMqttConStatus.Text = "MQTT Disconnected";
                }

            }
        }


    }
}
