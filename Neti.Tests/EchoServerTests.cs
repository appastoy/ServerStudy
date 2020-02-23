using System.Net;
using System.Text;
using FluentAssertions;
using Neti.Buffer;
using Neti.Clients;
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
            using (var server = new EchoServer())
            {
                server.Start(port);

                using (var client = new EchoClient())
                {
                    var sendMessage = "abc";
                    string receiveMessage = null;
                    
                    client.Connect(IPAddress.Loopback, port);
                    client.MessageReceived += msg => receiveMessage = msg;
                    client.SendMessage(sendMessage);

                    Waiting.Until(() => receiveMessage != null);

                    receiveMessage.Should().Be(sendMessage);
                }
            }
        }
    }
}
