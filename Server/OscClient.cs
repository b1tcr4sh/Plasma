using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BuildSoft.VRChat.Osc.Avatar;
using BuildSoft.VRChat.Osc;

namespace Plasma.Server {
    public class OscClient : IHostedService, IDisposable {
        private ILogger<OscClient> _logger;
        private OscAvatarConfig _avatarConfig;
        private bool _enabled;
        private SocketServer _server;

        public OscClient(SocketServer socketServer, IConfiguration config, ILogger<OscClient> logger) {
            _logger = logger;
            if (socketServer is null) throw new ArgumentNullException("Socket server was not passed to OSC client");
        }

        public bool receivingEnabled() => _enabled;
        public void onHeartRate(Object sender, PacketReceivedEventArgs args) {
            if (!_enabled) {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                _server.StopAsync(tokenSource.Token).GetAwaiter().GetResult();
                WaitForEnable();
                return;
            }

            OscParameter.SendAvatarParameter("Plasma/bpm", args.content);
            
        }

        public async Task StartAsync(CancellationToken token) {
            _logger.LogInformation("Starting OSC client");

            _avatarConfig = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();
            OscAvatarUtility.AvatarChanged += (param, args) => {
                _avatarConfig = OscAvatarConfig.CreateAtCurrent();
                WaitForValidAvatar();
            };            
            WaitForValidAvatar();
            _enabled = (bool) _avatarConfig.Parameters["Plasma/enabled"];

            OscParameter.SendAvatarParameter("Plasma/connected", true);

            if (!_enabled) {
                WaitForEnable();
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken serverToken = tokenSource.Token;

            await _server.StartAsync(serverToken);
            _server.PacketReceived += onHeartRate;
        }
        public Task StopAsync(CancellationToken token) {
            OscParameter.SendAvatarParameter("Plasma/connected", false);
            return Task.CompletedTask;
        }
        public void Dispose() {

        }
        
        private void WaitForEnable() {
            while (!_enabled) {
                _enabled = (bool) _avatarConfig.Parameters["Plasma/enable"]; 
                Thread.Sleep(1000);
            }
        }
        private void WaitForValidAvatar() {
            try {
                var enabled = _avatarConfig.Parameters["Plasma/enable"];
                var connected = _avatarConfig.Parameters["Plasma/connected"];
                var bpm = _avatarConfig.Parameters["Plasma/bpm"];
            } catch (KeyNotFoundException) {
                _logger.LogWarning("Current avatar doesn't have needed paramters...");
                Thread.Sleep(1000);
                WaitForValidAvatar();
            }
        }
    }
}