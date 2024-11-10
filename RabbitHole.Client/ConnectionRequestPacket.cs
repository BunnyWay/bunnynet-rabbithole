using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Serialization;

namespace RabbitHole.Client
{
    [JsonSerializable(typeof(ConnectionRequestPacket))]
    internal partial class ConnectionRequestPacketJsonContext : JsonSerializerContext { }

    public class ConnectionRequestPacket
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public ConnectionRequestPacket(string address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
