using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BuildSoft.VRChat.Osc.Avatar;
using BuildSoft.VRChat.Osc;

namespace Plasma.Server {
    public class OscClient : IHostedService, IDisposable {
        //TODO
        // Handle disable then reenable without having to reload avi

        private ILogger<OscClient> _logger;
        private OscAvatarConfig _avatarConfig;
        private bool _enabled = false;
        private SocketServer _server;

        public OscClient(SocketServer socketServer, IConfiguration config, ILogger<OscClient> logger) {
            _logger = logger;
            _server = socketServer;
            _server.PacketReceived += OnHeartRate;
            OscAvatarUtility.AvatarChanged += OnAviChanged;
        }

        public bool receivingEnabled() => _enabled;
        public async Task StartAsync(CancellationToken token) {
            _logger.LogInformation("Starting OSC client");

            _avatarConfig = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();     
            if (CheckAviValid()) {
                SetupAvi();
            }
            
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken serverToken = tokenSource.Token;

            await _server.StartAsync(serverToken);
        }
        public Task StopAsync(CancellationToken token) {
            OscParameter.SendAvatarParameter("Plasma/connected", false);
            return Task.CompletedTask;
        }
        public void Dispose() {

        }

        private void OnHeartRate(Object sender, PacketReceivedEventArgs args) {
            bool valid = CheckAviValid();

            if (!_enabled) {
                // CancellationTokenSource tokenSource = new CancellationTokenSource();

                // _server.StopAsync(tokenSource.Token).GetAwaiter().GetResult();
                // WaitForEnable();
                // if (valid) {
                //     WaitForEnable() -- Waits on every event thread
                // }

                return;
            }
            // Scale heartrate 30-200 - 0.1-1
            float normalized = (float) (args.content * 0.5) / 100;

            OscParameter.SendAvatarParameter("Plasma/bpm", normalized);
        }
        private void OnAviChanged(OscAvatar avi, ValueChangedEventArgs<OscAvatar> args) {
            _enabled = false;
            _avatarConfig = OscAvatarConfig.WaitAndCreateAtCurrentAsync().GetAwaiter().GetResult();
            if (!CheckAviValid()) {
                return;
            }

            SetupAvi();
        }
        private void SetupAvi() {
                _enabled = (bool) _avatarConfig.Parameters["Plasma/enabled"];
                OscParameter.SendAvatarParameter("Plasma/connected", true);
                _logger.LogInformation("Connected to avatar {0} over osc", _avatarConfig.Name);

                if (!_enabled) {
                    WaitForEnable();
                }
        }
        private void WaitForEnable() {
            while (!_enabled) {
                _enabled = (bool) _avatarConfig.Parameters["Plasma/enable"]; 
                Thread.Sleep(1000);
            }
        }
        private bool CheckAviValid() {
            try {
                var enabled = _avatarConfig.Parameters["Plasma/enable"];
                var connected = _avatarConfig.Parameters["Plasma/connected"];
                var bpm = _avatarConfig.Parameters["Plasma/bpm"];

                _logger.LogDebug("enabled is {0} by default", enabled);
                _logger.LogDebug("connected is {0} by default", connected);
                _logger.LogDebug("bpm is {0} by default", bpm);
            } catch (Exception) {
                return false;
            }
            if (_avatarConfig.Parameters["Plasma/enable"] is null) {
                _avatarConfig.Parameters["Plasma/enable"] = false;
            }
            if (_avatarConfig.Parameters["Plasma/connected"] is null) {
                _avatarConfig.Parameters["Plasma/connected"] = false;
            }
            return true;
        }
    }
}