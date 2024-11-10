using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace RabbitHole.Client
{
    public class ControlSession
    {
        private static ILogger _logger = Logger.Get<RabbitHoleClient>();

        public string Address { get; set; }
        public int Port { get; set; }
        public string AuthKey { get; set; }
        public long PullZoneId { get; set; }

        public Action<ConnectionRequestPacket> OnConnectionRequested;
        

        public ControlSession(string address, int port, long pullZoneId, string authKey, Action<ConnectionRequestPacket> onConnectionRequested)
        {
            Address = address;
            Port = port;
            AuthKey = authKey;
            PullZoneId = pullZoneId;
            OnConnectionRequested = onConnectionRequested;
        }

        public async void Start()
        {
            int failures = 0;
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // WebSocket server URI
                        string uri = $"ws://{Address}:8187";

                        // Create a new ClientWebSocket instance
                        using (var webSocket = new ClientWebSocket())
                        {
                            webSocket.Options.SetRequestHeader("AuthKey", AuthKey);
                            webSocket.Options.SetRequestHeader("PullZoneId", PullZoneId.ToString());
                            webSocket.Options.SetRequestHeader("BunnyCDN-RabbitHole", "Tunnel");

                            // Connect to the WebSocket server
                            _logger.LogInformation("Connecting to WebSocket server on " + uri);
                            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                            failures = 0;

                            _logger.LogInformation("Connected to WebSocket server on " + uri);
                            await RunWebSocketAsync(webSocket);
                        }
                    }
                    catch(Exception ex)
                    {
                        failures++;
                        _logger.LogError(ex, $"Failed to connect to WebSocket on {Address} on port {Port}. Waiting for {100 * failures}ms");
                        await Task.Delay(Math.Min(20000, 100 * failures));
                    }
                }
            });
        }

        private async Task RunWebSocketAsync(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 64];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        _logger.LogInformation("WebSocket connection closed.");
                        break;
                    }

                    // Convert received data to a string and print
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Session requested");
                    var connectionRequest = JsonSerializer.Deserialize<ConnectionRequestPacket>(message);
                    if (connectionRequest == null)
                    {
                        _logger.LogError("Failed to deserialize connection request packet");
                        continue;
                    }

                    OnConnectionRequested?.Invoke(connectionRequest);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                try
                {
                    webSocket.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose of WebSocket");
                }
            }
                
        }
    }
}