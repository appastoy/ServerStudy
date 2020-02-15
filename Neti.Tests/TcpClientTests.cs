using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Neti.Tests
{
    class TcpClientTests
    {
        const int testPort = 41717;

        TcpListener _listener;

        [SetUp]
        public void Setup()
        {
            _listener = new TcpListener(testPort);
            _listener.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _listener.Stop(); 
            _listener.Dispose();
        }

        [Test]
        public void ConnectToWrongAddress()
        {
            using (var client = new TcpClient(IPAddress.Any, testPort))
            {
                new Action(() => client.Connect()).Should().Throw<Exception>();
            }
        }

        [Test]
        public void Connect()
        {
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
            {
                var connectCheck = false;
                client.ConnectionChanged += isConnected => connectCheck = isConnected;
                connectCheck.Should().BeFalse();
                client.IsConnected.Should().BeFalse();

                client.Connect();

                connectCheck.Should().BeTrue();
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect()).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void ConnectAsync()
        {
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
            {
                var connectCheck = false;
                var connectionChanged = false;
                client.ConnectionChanged += isConnected => connectCheck = isConnected;
                client.ConnectionChanged += _ => connectionChanged = true;
                connectCheck.Should().BeFalse();
                client.IsConnected.Should().BeFalse();

                client.ConnectAsync();

                Task.Run(() => { while (connectionChanged == false) Thread.Yield(); }).Wait();
                connectCheck.Should().BeTrue();
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect()).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void Disconnect()
        {
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
            {
                var connectCheck = false;
                client.ConnectionChanged += isConnected => connectCheck = isConnected;
                var disconnectAction = new Action(() => client.Disconnect());

                client.IsConnected.Should().BeFalse();
                disconnectAction.Should().NotThrow();

                client.Connect();

                client.IsConnected.Should().BeTrue();
                connectCheck.Should().BeTrue();

                client.Disconnect();

                connectCheck.Should().BeFalse();
                client.IsConnected.Should().BeFalse();
                disconnectAction.Should().NotThrow();
            }
        }

        [Test]
        public void Dispose()
        {
            var connectCheck = false;
            var client = new TcpClient(IPAddress.Loopback, testPort);
            client.ConnectionChanged += isConnected => connectCheck = isConnected;
            var disconnectAction = new Action(() => client.Disconnect());

            client.Connect();

            connectCheck.Should().BeTrue();

            client.Dispose();

            connectCheck.Should().BeFalse();
            client.IsConnected.Should().BeFalse();
            disconnectAction.Should().NotThrow();
        }
    }
}
