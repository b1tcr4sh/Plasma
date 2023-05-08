using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;

namespace PlasmaOSCModule
{
    public partial class PlasmaOSCModule : ChatBoxModule
    {
        public override string Title => "PlasmaOSC";
        public override string Description => "Connects to Plasma client";
        public override string Author => "Kyra";
        public override ModuleType Type => ModuleType.General;
        protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(10);

        private SocketServer _server;
        private int _lastBpm;
        private bool _enabledParam;
        private bool _onlineParam;
        private int _minimumDelta;
        private bool _debug;

        protected override void CreateAttributes() {
            CreateSetting(PlasmaOSCSetting.ClientAddress, "Client Address", "Address of the hardware", string.Empty);
            CreateSetting(PlasmaOSCSetting.ClientPort, "Client Port", "Port of the hardware", string.Empty);
            CreateSetting(PlasmaOSCSetting.MinimumDelta, "Minimum BPM Delta", "Minimum difference between current and last BPM in order to update", 10);
            CreateSetting(PlasmaOSCSetting.DebugOutput, "Debug Output", "Toggles logging of each BPM packet", false);
            CreateParameter<int>(PlasmaOSCParameter.BPM, ParameterMode.Write, @"Plasma/BPM", "Heartrate BPM", "BPM value sent from client. Updates periodically.");
            CreateParameter<bool>(PlasmaOSCParameter.Enabled, ParameterMode.ReadWrite, @"Plasma/Enable", "Toggle", "Toggles whether or not BPM updates are sent.");
            CreateParameter<bool>(PlasmaOSCParameter.Online, ParameterMode.Write, @"Plasma/Online", "Online/Active", "Whether client is online and connected");
        }

        protected override void OnModuleStart() {
            _lastBpm = 0;
            _enabledParam = false;
            _onlineParam = false;

            string address = GetSetting<string>(PlasmaOSCSetting.ClientAddress);
            int port = GetSetting<int>(PlasmaOSCSetting.ClientPort);
            _minimumDelta = GetSetting<int>(PlasmaOSCSetting.MinimumDelta);
            _debug = GetSetting<bool>(PlasmaOSCSetting.DebugOutput);

            _server = new SocketServer(address, port);
            _server.PacketReceived += OnPacket;
            _server.ClientConnected += OnClientConnected;

            CancellationToken token = new CancellationTokenSource().Token;
            _server.StartAsync(token).GetAwaiter().GetResult();
            _onlineParam = true;
            SendParameter(PlasmaOSCParameter.Online, _onlineParam);
        }
        protected override void OnModuleUpdate() {
            if (_enabledParam) {
                SendParameter(PlasmaOSCParameter.BPM, _lastBpm);
                if (_debug) {
                    Log("Sent " + _lastBpm);
                }
            }
        }
        protected override void OnModuleStop() {
            CancellationToken token = new CancellationTokenSource().Token;
            _server.StopAsync(token).GetAwaiter().GetResult();
            _server.Dispose();
        }
        // protected override void OnBoolParameterReceived(PlasmaOSCParameter key, bool value) {
        //     if (key == PlasmaOSCParameter.Enabled) {
        //         _enabledParam = value;
        //     }
        // }
        protected override void OnBoolParameterReceived(Enum key, bool value) {
            if (key.Equals(PlasmaOSCParameter.Enabled)) {
                _enabledParam = value;
            }
        }

        private void OnPacket(object sender, PacketReceivedEventArgs e) {
            int bpm = int.Parse(e.content);
            
            if (Math.Abs(bpm - _lastBpm) > _minimumDelta) {
                _lastBpm = bpm;
            }
            
            if (_debug) {
                Console.WriteLine(e.content);
            }
        } 
        private void OnClientConnected(object sender, ClientConnectedEventArgs e) {
            Log("Conected to " + (int) e.address);
        }

        protected enum PlasmaOSCSetting {
            ClientAddress,
            ClientPort,
            UpdateInterval,
            MinimumDelta,
            DebugOutput
        }

        protected enum PlasmaOSCParameter {
            BPM,
            Enabled,
            Online
        }
    }
}
