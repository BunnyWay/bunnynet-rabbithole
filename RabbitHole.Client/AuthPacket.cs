using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RabbitHole.Client
{

    [JsonSerializable(typeof(AuthPacket))]
    internal partial class AuthPacketJsonContext : JsonSerializerContext { }

    public class AuthPacket
    {
        public long PullZoneId { get; set; }
        public string AuthToken { get; set; }
        public bool IsControlSession { get; set; }

        public AuthPacket(long pullZoneId, string authToken, bool isControlSession)
        {
            PullZoneId = pullZoneId;
            AuthToken = authToken;
            IsControlSession = isControlSession;
        }
    }
}
