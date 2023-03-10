using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Text;
using NLog;
using System.Threading.Tasks;

namespace Plasma {
    public delegate void PacketReceived(object sender, PacketReceivedEventArgs e);
    public class SocketServer : IHostedService, IDisposable {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        private Socket socket;
        private string address;
        private int port;
        private ILogger _logger;

        public SocketServer(IConfiguration config) {
            // _logger = NLog.LogManager.GetCurrentClassLogger();

            try {
                address = config["address"];
                port = Int32.Parse(config["port"]);

            } catch (Exception e) {
                _logger.Error(e.Message);
                _logger.Trace(e.StackTrace);
            }
        }

        public async Task StartAsync(CancellationToken token) {
            if (address is null || port == 0) {
                throw new ArgumentNullException("Arguments to SocketServer were null!");
            }
            
            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress)) {
                Console.Error.WriteLine("IP address was malformed!");
                throw new Exception("");
            }

            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(endPoint);
            socket.Listen(100);
            Socket handler = await socket.AcceptAsync();
            
            // _logger.Debug("Listening for TCP sockers on {0}:{1}...", address, port);
            Console.WriteLine("Listening for TCP sockers on {0}:{1}...", address, port);
            await WaitForPacketAsync(handler);
        }
        private async Task WaitForPacketAsync(Socket handler) {
            Byte[] buffer = new byte[1_024];
            int resLength = await handler.ReceiveAsync(buffer, SocketFlags.None);
            string res = Encoding.UTF8.GetString(buffer, 0, resLength);
            await handler.SendAsync(Encoding.UTF8.GetBytes("<|ACK|>"));

            PacketReceived.Invoke(this, new PacketReceivedEventArgs(res));
            await WaitForPacketAsync(handler);
        }
        public Task StopAsync(CancellationToken token) {
            socket.Close();
            return Task.CompletedTask;
        }
        public void Dispose() {

        }
    }
    public class PacketReceivedEventArgs : EventArgs {
        public string content { get; set; }
        public PacketReceivedEventArgs(string content) : base() {
            this.content = content;
        }
    }
}