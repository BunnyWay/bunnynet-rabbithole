using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text;

namespace RabbitHole.Client
{
    public class RabbitHoleClient
    {
        private static ILogger _logger = Logger.Get<RabbitHoleClient>();

        private List<RabbitHoleSession> _sessions = new List<RabbitHoleSession>();

        private ControlSession? _controlSession;
        public long PullZoneId { get; }
        public string ApiKey { get; }
        public bool SessionStarted { get; set; }

        public string LocalIP { get; set; }
        public int LocalPort { get; set; }
        public string TunnelHostname { get; set; }
        public int TunnelPort { get; set; }
        public int TunnelControlPort { get; set; }

        public RabbitHoleClient(long pullZoneId, string apiKey, string localIP, int localPort, string tunnelHostname, int tunnelPort, int tunnelControlPort)
        {
            PullZoneId = pullZoneId;
            ApiKey = apiKey;
            LocalIP = localIP;
            LocalPort = localPort;
            TunnelHostname = tunnelHostname;
            TunnelPort = tunnelPort;
            TunnelControlPort = tunnelControlPort;
        }

        public void Start()
        {
            // Start the control session, which will be responsible for creating new data sessions
            _controlSession = new ControlSession(TunnelHostname, TunnelControlPort, PullZoneId, ApiKey, AddSession);
            _controlSession.Start();
        }

        private void AddSession(ConnectionRequestPacket packet)
        {
            var s = new RabbitHoleSession(PullZoneId, ApiKey, LocalIP, LocalPort, TunnelHostname, TunnelPort);
            lock(_sessions)
            {
                _sessions.Add(s);
            }
            s.OnSessionDisconnected = () =>
            {
                lock(_sessions)
                {
                    _sessions.Remove(s);
                }
                _logger.LogInformation($"Session disconnected. Current session count: {_sessions.Count}");
            };

            s.StartAsync();
            _logger.LogInformation($"Adding new session. Total session count: {_sessions.Count}");
        }

        public static byte[] GetAuthPacket(long pullZoneId, string authToken)
        {
            var json = JsonSerializer.Serialize(new AuthPacket(
                pullZoneId,
                authToken,
                false
            ), AuthPacketJsonContext.Default.AuthPacket);

            var bytes = Encoding.UTF8.GetBytes(json);

            // Write 4 bytes of length of the bytes using BitConverter
            var length = BitConverter.GetBytes(bytes.Length);
            var buffer = new byte[bytes.Length + 4];

            length.CopyTo(buffer, 0);
            bytes.CopyTo(buffer, 4);

            return buffer;
        }
    }
}
