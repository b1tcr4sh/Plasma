using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BuildSoft.VRChat.Osc.Avatar;
using BuildSoft.VRChat.Osc;

namespace Plasma.Server {
    public class OscClient : IHostedService, IDisposable {
        private ILogger<OscClient> _logger;
        private OscAvatarConfig avatarConfig;
        private bool enabled;

        public OscClient(IConfiguration config, ILogger<OscClient> logger) {
            _logger = logger;
        }

        public bool receivingEnabled() => enabled;
        public bool sendHeartRate(int bpm) {
            if (!enabled) return false;

            OscParameter.SendAvatarParameter("Plasma/bpm", bpm);
            return true;
        }

        public async Task StartAsync(CancellationToken token) {
            avatarConfig = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();
            OscAvatarUtility.AvatarChanged += (param, args) => {
                avatarConfig = OscAvatarConfig.CreateAtCurrent();
            };
            enabled = (bool) avatarConfig.Parameters["Plasma/enable"]; // Doesn't handle if the avatar doesn't have the param
            while (!enabled) {
                enabled = (bool) avatarConfig.Parameters["Plasma/enable"]; 
            }
        }
        public async Task StopAsync(CancellationToken token) {

        }
        public void Dispose() {

        }
        
    }
}