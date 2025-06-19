using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace RabbitHole.Client
{
    public class TcpPairSession
    {
        private static ILogger _logger = Logger.Get<TcpPairSession>();

        private bool _firstDataReceived = false;
        private Stream _stream;

        public Action? OnFirstDataReceived { get; set; }
        public Action? OnSessionDisconnected { get; set; }

        public byte[]? InitialMessage { get; set; }

        public TcpPairSession? Pair { get; set; }

        private byte[] _buffer;
        private TcpClient? TcpClient { get; set; }

        public string Hostname { get; set; }
        public int Port { get; set; }

        public TcpPairSession(string address, int port, byte[]? initialMessage = null)
        {
            Hostname = address;
            Port = port;

            InitialMessage = initialMessage;
            _buffer = new byte[1024 * 256];
        }

        public void StartSession()
        {
            try
            {
                _logger.LogInformation($"Connecting to {Hostname} on port {Port}");
                TcpClient = new TcpClient(Hostname, Port);
                _stream = TcpClient.GetStream();
                _logger.LogInformation($"Successfully connected to {Hostname} on port {Port}, starting data worker");

                Task.Factory.StartNew(SessionReadWorker, TaskCreationOptions.LongRunning);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to connect to {Hostname} on port {Port}");
            }
        }

        private async Task WaitForPairAsync()
        {
            while (Pair == null)
            {
                await Task.Delay(25);
            }
        }

        private async void SessionReadWorker()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(1024 * 64);
            try
            {
                await WaitForPairAsync();

                if (InitialMessage != null)
                {
                    await _stream.WriteAsync(InitialMessage, 0, InitialMessage.Length);
                }

                while (true)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        throw new Exception($"Received 0 bytes from session, disconnecting");
                    }

                    if (!_firstDataReceived)
                    {
                        try
                        {

                            OnFirstDataReceived?.Invoke();
                        }
                        catch { }
                        _firstDataReceived = true;
                    }

                    if (Pair != null)
                    {
                        await Pair.TryWriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Session worker failed for {Hostname} on port {Port}");
                HandleDisconnect(true);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        public async Task TryWriteAsync(byte[] data, int offset, long size)
        {
            try
            {
                await _stream.WriteAsync(data, offset, (int)size);
            }
            catch(Exception ex)
            {
                HandleDisconnect(true);
            }
        }

        private void HandleDisconnect(bool disconnectPair)
        {
            try
            {
                if(disconnectPair)
                {
                    Pair?.HandleDisconnect(false);
                }
                try
                {
                    _stream?.Dispose();
                }
                catch { }
                try
                {
                    TcpClient?.Dispose();
                }
                catch { }

                OnSessionDisconnected?.Invoke();
            }
            catch(Exception ex) { }
        }
    }
}
