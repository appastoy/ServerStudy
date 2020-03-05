using System.Net;
using FluentAssertions;
using Neti.Echo;
using Neti.Echo.Client;
using Neti.Echo.Server;
using Neti.Packets;
using NUnit.Framework;

namespace Neti.Tests
{
	class EchoTests
	{
        [Test]
        public void EchoMessage()
        {
            int port = 41717;
            using (var server = new EchoServer())
            {
                server.Start(port);

                using (var client = new EchoClient())
                {
                    var sendMessage = "abc";
                    string receiveMessage = null;
                    
                    client.Connect(IPAddress.Loopback, port);
                    client.PacketReceived += reader =>
                    {
                        reader.Reset();
                        reader.Read<ushort>();
                        receiveMessage = reader.ReadString();
                    };
                    ClientToServer.Rpc.RequestEcho(client, sendMessage);
                    client.FlushPackets();

                    Waiting.Until(() => receiveMessage != null);

                    receiveMessage.Should().Be(sendMessage);
                }
            }
        }
    }
}
