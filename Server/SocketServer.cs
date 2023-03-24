using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Plasma.Server {
    public delegate void PacketReceived(object sender, PacketReceivedEventArgs e);
    public class SocketServer : IHostedService, IDisposable {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        private Socket listenerSock;
        private Socket handlerSock;
        private CancellationTokenSource waitCancellation;
        private string address;
        private int port;
        private readonly ILogger _logger;

        public SocketServer(IConfiguration config, ILogger<SocketServer> logger) {
            _logger = logger;
            // _logger = 
            try {
                address = config["address"];
                port = Int32.Parse(config["port"]);

            } catch (Exception e) {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        public async Task StartAsync(CancellationToken token) {
            if (address is null || port == 0) {
                throw new ArgumentNullException("Arguments to SocketServer were null!");
            }
            
            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress)) {
                throw new Exception("IP address was malformed!");
            }

            PacketReceived += OnPacket;

            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            listenerSock = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenerSock.Bind(endPoint);
            listenerSock.Listen(100);

            _logger.LogInformation("Listening for TCP connections on {0}:{1}...", address, port);
            handlerSock = await listenerSock.AcceptAsync();

            waitCancellation = new CancellationTokenSource();
            await WaitForPacketAsync(handlerSock, waitCancellation.Token);
        }
        private async Task WaitForPacketAsync(Socket handler, CancellationToken cancellation) {
            Byte[] buffer = new byte[1_024];
            int resLength = await handler.ReceiveAsync(buffer, SocketFlags.None, cancellation);
            string res = Encoding.UTF8.GetString(buffer, 0, resLength);
            handler.Send(Encoding.UTF8.GetBytes("<|ACK|>"), SocketFlags.None);

            PacketReceived.Invoke(this, new PacketReceivedEventArgs(res));
            await WaitForPacketAsync(handler, cancellation);
        }
        public Task StopAsync(CancellationToken token) {
            waitCancellation.Cancel();
            waitCancellation.Dispose();
            _logger.LogInformation("Closing sockets...");
            listenerSock.Disconnect(true);
            handlerSock.Disconnect(false);
            return Task.CompletedTask;
        }
        public void Dispose() {
            Dispose(true);
        }
        private void Dispose(bool disposing) {
            _logger.LogInformation("Disposing socket server...");
            if (disposing) {
                if (listenerSock is not null && listenerSock.Connected) {
                    listenerSock.Disconnect(false);
                }
                if (handlerSock is not null && handlerSock.Connected) {
                    handlerSock.Disconnect(false);
                }

                listenerSock.Shutdown(SocketShutdown.Both);
                handlerSock.Shutdown(SocketShutdown.Both);
                _logger.LogInformation("Giving sockets connection 10 seconds to close...");
                listenerSock.Close(5000);
                handlerSock.Close(5000);
            }
        }

        private void OnPacket(object sender, PacketReceivedEventArgs args) {
            _logger.LogInformation("Received: " + args.content);
        }
    }
    public class PacketReceivedEventArgs : EventArgs {
        public string content { get; set; }
        public PacketReceivedEventArgs(string content) : base() {
            this.content = content;
        }
    }
}