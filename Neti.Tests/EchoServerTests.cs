using System.Net;
using System.Text;
using FluentAssertions;
using Neti.Buffer;
using Neti.Servers;
using NUnit.Framework;

namespace Neti.Tests
{
	class EchoServerTests
	{
        [Test]
        public void EchoMessage()
        {
            int port = 41717;
            using (var server = new EchoServer(port))
            {
                server.Start();

                using (var client = new TcpClient(IPAddress.Loopback, port))
                {
                    byte[] receivedData = null;
                    var sendData = Encoding.UTF8.GetBytes("abc");

                    client.Connect();
                    client.BytesReceived += reader =>
                    {
                        receivedData = BufferUtility.CloneBuffer(reader.Buffer, reader.ReadPosition, reader.ReadableSize);
                        reader.ExternalRead(reader.ReadableSize);
                    };
                    client.Send(sendData);

                    Waiting.Until(() => receivedData != null);

                    receivedData.Should().BeEquivalentTo(sendData);
                }
            }
        }
    }
}
