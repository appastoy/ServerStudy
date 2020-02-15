using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Neti
{
    static class PortUtility
    {
        const int _minPort = 49152;
        const int _maxPort = IPEndPoint.MaxPort;

        public static void ValidatePort(int port)
        {
            if (port <= 0 || port > _maxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port), port, $"port should be over 0 and under {IPEndPoint.MaxPort}.");
            }
        }

        public static int? FindAvailableTcpPort()
        {
            var usedPorts = CollectUsedTcpPorts();
            return FindUnusedPort(usedPorts);
        }

        public static int? FindAvailableUdpPort()
        {
            var usedPorts = CollectUsedUdpPorts();
            return FindUnusedPort(usedPorts);
        }

        public static int[] FindAvailableTcpPorts()
        {
            var usedPorts = CollectUsedTcpPorts();
            return FindUnusedPorts(usedPorts);
        }

        public static int[] FindAvailableUdpPorts()
        {
            var usedPorts = CollectUsedUdpPorts();
            return FindUnusedPorts(usedPorts);
        }

        static int? FindUnusedPort(int[] usedPorts)
        {
            for (int i = _minPort; i <= _maxPort; i++)
            {
                if (usedPorts.Contains(i) == false)
                {
                    return i;
                }
            }

            return null;
        }

        static int[] FindUnusedPorts(int[] usedPorts)
        {
            return Enumerable.Range(_minPort, _maxPort - _minPort)
                             .Where(port => usedPorts.Contains(port) == false)
                             .ToArray();
        }

        static int[] CollectUsedTcpPorts()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var connectionPorts = properties.GetActiveTcpConnections()
                                            .Where(connection => connection.LocalEndPoint.Port >= _minPort)
                                            .Select(connection => connection.LocalEndPoint.Port);
            var activePorts = properties.GetActiveTcpListeners()
                                        .Where(listener => listener.Port >= _minPort)
                                        .Select(listener => listener.Port);
            return connectionPorts.Concat(activePorts)
                                  .OrderBy(port => port)
                                  .ToArray();
        }

        static int[] CollectUsedUdpPorts()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var activePorts = properties.GetActiveUdpListeners()
                                        .Where(listener => listener.Port >= _minPort)
                                        .Select(listener => listener.Port);
            return activePorts.OrderBy(port => port)
                              .ToArray();
        }
    }
}
