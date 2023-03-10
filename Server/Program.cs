using System.Net.Sockets;
using System.Net;
using System.Text;

public class Plasma {
    public static async Task Main(string[] args) {
        Console.Title = "Plasma Socket -> OSC Server";
        
        IPAddress address = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(address, 3012);

        using Socket listener = new(
        endPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

        listener.Bind(endPoint);
        listener.Listen(100);

        Console.WriteLine("Listening...");
        var handler = await listener.AcceptAsync();
        while (true)
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            
            var eom = "<|EOM|>";
            // if (response.IndexOf(eom) > -1 /* is end of message */)
            // {
                Console.WriteLine(
                    $"Socket server received message: \"{response.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await handler.SendAsync(echoBytes, 0);
                // Console.WriteLine(
                //     $"Socket server sent acknowledgment: \"{ackMessage}\"");

                // break;
            // }
        }
    }
}

