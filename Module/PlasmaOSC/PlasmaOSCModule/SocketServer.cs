using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PlasmaOSCModule {
    public delegate void PacketReceived(object sender, PacketReceivedEventArgs e);
    public class SocketServer : IDisposable {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        private Socket _listenerSock;
        private Socket _handlerSock;
        private CancellationTokenSource _waitCancellation;
        private string _address;
        private int _port;

        public SocketServer(string address, int port) {
            _address = address;
            _port = port;
        }

        public async Task StartAsync(CancellationToken token) {
            if (_address is null || _port == 0) {
                throw new ArgumentNullException("Arguments to SocketServer were null!");
            }
            
            IPAddress ipAddress;
            if (!IPAddress.TryParse(_address, out ipAddress)) {
                throw new Exception("IP address was malformed!");
            }
            IPEndPoint endPoint = new IPEndPoint(ipAddress, _port);
            _listenerSock = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listenerSock.Bind(endPoint);
            _listenerSock.Listen(100);

            // _logger.LogInformation("Listening for TCP connections on {0}:{1}...", _address, _port);
            _handlerSock = await _listenerSock.AcceptAsync();
            // _logger.LogInformation("Connected to a socket!");

            _waitCancellation = new CancellationTokenSource();
            await WaitForPacketAsync(_handlerSock, _waitCancellation.Token);
        }
        public Task StopAsync(CancellationToken token) {
            if (_handlerSock is not null && _listenerSock is not null) {
                _handlerSock.Shutdown(SocketShutdown.Both);
                _handlerSock.Close(1000);
                _listenerSock.Shutdown(SocketShutdown.Both);
                _listenerSock.Close();
            }
            return Task.CompletedTask;
        }
        public void Dispose() {
            Dispose(true);
        }
        private void Dispose(bool disposing) {
            if (!disposing) {
                if (_listenerSock is not null && _listenerSock.Connected) {
                    _listenerSock.Disconnect(false);
                }
                if (_handlerSock is not null && _handlerSock.Connected) {
                    _handlerSock.Disconnect(false);
                }
            }
        }
        private async Task WaitForPacketAsync(Socket handler, CancellationToken cancellation) {
            Byte[] buffer = new byte[1_024];
            int resLength = await handler.ReceiveAsync(buffer, SocketFlags.None, cancellation);
            string res = Encoding.UTF8.GetString(buffer, 0, resLength);
            handler.Send(Encoding.UTF8.GetBytes("<|ACK|>"), SocketFlags.None);

            PacketReceived.Invoke(this, new PacketReceivedEventArgs(res));
            await Task.Delay(1000);
            await WaitForPacketAsync(handler, cancellation);
        }
    }
    public class PacketReceivedEventArgs : EventArgs {
        public string content { get; set; }
        public PacketReceivedEventArgs(string content) : base() {
            this.content = content;
        }
    }
}