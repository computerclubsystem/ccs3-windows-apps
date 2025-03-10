using Ccs3HttpToUdpProxyWindowsService.Entities;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace Ccs3HttpToUdpProxyWindowsService;

public class PacketsSender {
    private SendPacketsRequest? _pendingRequest;
    private bool _isSending = false;
    private readonly object _lock = new object();
    private readonly ILogger _logger;
    public PacketsSender(ILogger logger) {
        _logger = logger;
    }

    public async void SendUdpPackets(SendPacketsRequest payload) {
        List<PacketData?> packetItems = new();
        lock (_lock) {
            if (_isSending) {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    LogInformation("Request arrived while processing another one");
                }
                _pendingRequest = payload;
                return;
            }
            _isSending = true;
            // Generate table-like structure to achieve order with different IP addresses to minimize delays
            var groups = payload.PacketsData.GroupBy(x => x.DestinationIpAddress).ToArray();
            var maxItemsCount = groups.Max(x => x.Count());
            for (int row = 0; row < maxItemsCount; row++) {
                for (int col = 0; col < groups.Length; col++) {
                    var grp = groups[col];
                    var el = grp.ElementAtOrDefault(row);
                    if (el != null) {
                        packetItems.Add(el);
                    }
                }
                // After each row we need to have null element to mark delay
                packetItems.Add(null);
            }
            // Remove the last item, because it will be null - we don't need to delay after the last item is sent
            packetItems.RemoveAt(packetItems.Count - 1);
        }
        using UdpClient udpClient = new UdpClient();
        if (_logger.IsEnabled(LogLevel.Information)) {
            string message = "Start processing. Elements count " + payload.PacketsData.Length.ToString();
            LogInformation(message);
        }
        for (int i = 0; i < packetItems.Count; i++) {
            var packetData = packetItems[i];
            if (packetData != null) {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    string message = $"Sending to {packetData.DestinationIpAddress}:{packetData.DestinationPort} {packetData.PacketHexString}";
                    LogInformation(message);
                }
                try {
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(packetData.DestinationIpAddress), packetData.DestinationPort);
                    byte[] packetBytes = Convert.FromHexString(packetData.PacketHexString);
                    await udpClient.SendAsync(packetBytes, packetBytes.Length, endpoint);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error occured when sending the UDP packet");
                }
            } else {
                // This is null element - this means delay
                if (payload.DelayBetweenPacketsMilliseconds > 0) {
                    await Task.Delay(payload.DelayBetweenPacketsMilliseconds);
                }
            }
        }
        if (_logger.IsEnabled(LogLevel.Information)) {
            LogInformation("Processing finished");
        }
        SendPacketsRequest? pendingReq = null;
        lock (_lock) {
            if (_pendingRequest != null) {
                if (_logger.IsEnabled(LogLevel.Information)) {
                    LogInformation("Processing pending request");
                }
                pendingReq = _pendingRequest;
                _pendingRequest = null;
            }
            _isSending = false;
        }
        if (pendingReq != null) {
            SendUdpPackets(pendingReq);
        }
    }

    private void LogInformation(string message) {
        _logger.LogInformation($"{DateTime.Now.ToString("O")} {message}");
    }
}
