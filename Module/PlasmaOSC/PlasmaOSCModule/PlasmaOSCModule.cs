using VRCOSC.Game.Modules;

namespace PlasmaOSCModule
{
    public partial class PlasmaOSCModule : Module
    {
        public override string Title => "PlasmaOSC";
        public override string Description => "Connects to Plasma client";
        public override string Author => "Kyra";
        public override ModuleType Type => ModuleType.General;
        protected override TimeSpan DeltaUpdate => TimeSpan.MaxValue;

        private SocketServer _server;

        protected override void CreateAttributes()
        {
            CreateSetting(PlasmaOSCSetting.ClientAddress, "Client Address", "Address of the hardware", string.Empty);
            CreateSetting(PlasmaOSCSetting.ClientPort, "Client Port", "Port of the hardware", string.Empty);
            CreateParameter<int>(PlasmaOSCParameter.BPM, ParameterMode.Write, @"Plasma/BPM", "Heartrate BPM", "BPM value sent from client. Updates periodically.");
            CreateParameter<bool>(PlasmaOSCParameter.Enabled, ParameterMode.ReadWrite, @"Plasma/Enable", "Toggle", "Toggles whether or not BPM updates are sent.");
            CreateParameter<bool>(PlasmaOSCParameter.Online, ParameterMode.Write, @"Plasma/Online", "Online/Active", "Whether client is online and connected");
        }

        protected override void OnModuleStart()
        {
            string address = GetSetting<string>(PlasmaOSCSetting.ClientAddress);
            int port = GetSetting<int>(PlasmaOSCSetting.ClientPort);

            _server = new SocketServer(address, port);
            _server.PacketReceived += OnPacket;

            CancellationToken token = new CancellationTokenSource().Token;
            _server.StartAsync(token).GetAwaiter().GetResult();
        }
        protected override void OnModuleUpdate()
        {
        }
        protected override void OnModuleStop()
        {
            CancellationToken token = new CancellationTokenSource().Token;
            _server.StopAsync(token).GetAwaiter().GetResult();
            _server.Dispose();
        }

        private void OnPacket(object sender, PacketReceivedEventArgs e) {
            Console.WriteLine(e.content);
        } 

        private enum PlasmaOSCSetting
        {
            ClientAddress,
            ClientPort
        }

        private enum PlasmaOSCParameter
        {
            BPM,
            Enabled,
            Online
        }
    }
}
