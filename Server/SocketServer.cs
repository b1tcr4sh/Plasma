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

        public SocketServer(IConfiguration config, ILogger<SocketServer> logger, IHostApplicationLifetime appLifetime) {
            _logger = logger;
            try {
                address = config["address"];
                port = Int32.Parse(config["port"]);

            } catch (KeyNotFoundException) {
                _logger.LogCritical("Missing arguments for server address/port");
                Environment.Exit(-1);
            } catch (ArgumentNullException) {
                _logger.LogCritical("Missing required arguments!");
                Environment.Exit(-1);
            }

            appLifetime.ApplicationStopping.Register(OnStopping);
        }

        public async Task StartAsync(CancellationToken token) {
            if (address is null || port == 0) {
                throw new ArgumentNullException("Arguments to SocketServer were null!");
            }
            
            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress)) {
                throw new Exception("IP address was malformed!");
            }
#if DEBUG 
            PacketReceived += OnPacket;
#endif
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            listenerSock = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenerSock.Bind(endPoint);
            listenerSock.Listen(100);

            _logger.LogInformation("Listening for TCP connections on {0}:{1}...", address, port);
            handlerSock = await listenerSock.AcceptAsync();
            _logger.LogInformation("Connected to a socket!");

            waitCancellation = new CancellationTokenSource();
            await WaitForPacketAsync(handlerSock, waitCancellation.Token);
        }
        public Task StopAsync(CancellationToken token) {
            
            return Task.CompletedTask;
        }
        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool disposing) {
            if (!disposing) {
                if (listenerSock is not null && listenerSock.Connected) {
                    listenerSock.Disconnect(false);
                }
                if (handlerSock is not null && handlerSock.Connected) {
                    handlerSock.Disconnect(false);
                }
            }
        }
        private async Task WaitForPacketAsync(Socket handler, CancellationToken cancellation) {
            Byte[] buffer = new byte[1_024];
            int resLength = await handler.ReceiveAsync(buffer, SocketFlags.None, cancellation);
            string res = Encoding.UTF8.GetString(buffer, 0, resLength);
            handler.Send(Encoding.UTF8.GetBytes("<|ACK|>"), SocketFlags.None);

            PacketReceived.Invoke(this, new PacketReceivedEventArgs(int.Parse(res)));
            await Task.Delay(1000);
            await WaitForPacketAsync(handler, cancellation);
        }
        private void OnPacket(object sender, PacketReceivedEventArgs args) {
            _logger.LogDebug("Received: " + args.content);
        }
        private void OnStopping() {
            _logger.LogInformation("Closing socket server");

            if (handlerSock is not null && listenerSock is not null) {
                handlerSock.Shutdown(SocketShutdown.Both);
                handlerSock.Close(1000);
                listenerSock.Shutdown(SocketShutdown.Both);
                listenerSock.Close();
            }
        }
    }
    public class PacketReceivedEventArgs : EventArgs {
        public int content { get; set; }
        public PacketReceivedEventArgs(int content) : base() {
            this.content = content;
        }
    }
}