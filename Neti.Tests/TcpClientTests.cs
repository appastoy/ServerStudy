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
        const int _testPort = 41717;

        TcpListener _listener;
        TcpClient _connectedClient;

        [SetUp]
        public void Setup()
        {
            _listener = new TcpListener();
            _listener.NewClientEntered += client => _connectedClient = TcpClient.CreateFromSocket(client);
            _listener.Start(_testPort);
        }

        [TearDown]
        public void TearDown()
        {
            _connectedClient?.Dispose();
            _connectedClient = null;
            _listener.Stop(); 
            _listener.Dispose();
            _listener = null;
        }

        [Test]
        public void ConnectToWrongAddress()
        {
            using (var client = new TcpClient())
            {
                new Action(() => client.Connect(IPAddress.Any, _testPort)).Should().Throw<Exception>();
            }
        }

        [Test]
        public void Connect()
        {
            using (var client = new TcpClient())
            {
                var connectCheck = false;
                client.Connected += () => connectCheck = true;
                connectCheck.Should().BeFalse();
                client.IsConnected.Should().BeFalse();

                client.Connect(IPAddress.Loopback, _testPort);

                connectCheck.Should().BeTrue();
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect(IPAddress.Loopback, _testPort)).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void ConnectAsync()
        {
            using (var client = new TcpClient())
            {
                var connectCheck = false;
                client.Connected += () => connectCheck = true;
                client.IsConnected.Should().BeFalse();

                client.ConnectAsync(IPAddress.Loopback, _testPort);

                Waiting.Until(() => connectCheck);
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect(IPAddress.Loopback, _testPort)).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void Send()
        {
            using (var client = new TcpClient())
            {
                ArraySegment<byte>? bytes = new ArraySegment<byte>();
                var sendData = new byte[] { 123 };

                client.Connect(IPAddress.Loopback, _testPort);

                Waiting.Until(() => _connectedClient != null);
                _connectedClient.BytesReceived += buffer => bytes = buffer; 

                client.Send(sendData);

                Waiting.Until(() => bytes.HasValue);
                bytes.Value.Array.Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void SendAsync()
        {
            using (var client = new TcpClient())
            {
                ArraySegment<byte> bytes = null;
                var sendData = new byte[] { 234 };

                client.Connect(IPAddress.Loopback, _testPort);

                Waiting.Until(() => _connectedClient != null);
                _connectedClient.BytesReceived += buffer => bytes = buffer;

                client.SendAsync(sendData);

                Waiting.Until(() => bytes != null);
                bytes.Array.Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void Disconnect()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, _testPort);
                client.Disconnect();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeFalse();
                new Action(() => client.Disconnect()).Should().NotThrow();
            }
        }

        [Test]
        public void DisconnectAndClose()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, _testPort);
                client.DisconnectAndClose();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.DisconnectAndClose()).Should().NotThrow();
            }
        }

        [Test]
        public void DisconnectAync()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, _testPort);
                client.DisconnectAsync();

                Waiting.Until(() => disconnectCheck);
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeFalse();
                new Action(() => client.DisconnectAsync()).Should().NotThrow();
            }
        }

        [Test]
        public void DisconnectAndCloseAync()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;
                
                client.Connect(IPAddress.Loopback, _testPort);
                client.DisconnectAndCloseAsync();

                Task.Run(() => { while (disconnectCheck == false) Thread.Yield(); }).Wait();
                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.DisconnectAndCloseAsync()).Should().NotThrow();
            }
        }

        [Test]
        public void Close()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, _testPort);
                client.Close();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.Close()).Should().NotThrow();
            }
        }

        [Test]
        public void Dispose()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, _testPort);
                client.Dispose();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.Dispose()).Should().NotThrow();
            }
        }
    }
}
