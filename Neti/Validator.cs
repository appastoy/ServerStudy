using System;
using System.Net;

namespace Neti
{
    public static class Validator
	{
		public static void ValidateBytes(byte[] bytes, int offset, int count)
		{
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (count < 0 ||
                count > bytes.Length)
            {
                throw new ArgumentException($"Invalid count({count}).");
            }

            if (offset < 0 ||
               (offset + count) > bytes.Length)
            {
                throw new ArgumentException($"Invalid offset({offset}).");
            }
        }

        public static void ValidatePort(int port)
        {
            if (port < 0 || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port), port, $"Invalid port. port must be over 0 and under {IPEndPoint.MaxPort}.");
            }
        }

        public static bool IsValidPort(int port)
        {
            return port >= 0 && port <= IPEndPoint.MaxPort;
        }

        public static IPAddress ValidateAndParseIp(string ip)
        {
            if (ip is null)
            {
                throw new ArgumentNullException(nameof(ip));
            }

            if (IPAddress.TryParse(ip, out var ipAddress))
            {
                throw new ArgumentException("Invalid ip.");
            }

            return ipAddress;
        }
    }
}
