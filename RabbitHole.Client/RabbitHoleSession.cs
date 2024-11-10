using System.Net;
using System.Text;
using System.Text.Json;

namespace RabbitHole.Client
{
    public class RabbitHoleSession
    {
        public long PullZoneId { get; }
        public string ApiKey { get; }

        public bool SessionStarted { get; set; }

        public TcpPairSession LocalTcpSession { get; set; }
        public TcpPairSession BunnyTcpSession { get; set; }
        public Action? OnSessionStarted { get; set; }
        public Action? OnSessionDisconnected { get; set; }

        public RabbitHoleSession(long pullZoneId, string apiKey, string localIP, int localPort, string tunnelHostname, int tunnelPort)
        {
            PullZoneId = pullZoneId;
            ApiKey = apiKey;

            var authPacket = RabbitHoleClient.GetAuthPacket(pullZoneId, apiKey);

            LocalTcpSession = new TcpPairSession(localIP, localPort);
            BunnyTcpSession = new TcpPairSession(tunnelHostname, tunnelPort, authPacket);

            LocalTcpSession.Pair = BunnyTcpSession;
            BunnyTcpSession.Pair = LocalTcpSession;

            BunnyTcpSession.OnFirstDataReceived = () =>
            {
                SessionStarted = true;
                try
                {
                    OnSessionStarted?.Invoke();
                }
                catch { }
            };
            BunnyTcpSession.OnSessionDisconnected = () =>
            {
                try
                {
                    OnSessionDisconnected?.Invoke();
                }
                catch { }
            };
        }


        public void StartAsync()
        {
            BunnyTcpSession.StartSession();
            LocalTcpSession.StartSession();
        }
    }
}
