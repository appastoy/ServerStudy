using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Neti.Buffer;
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
            _listener = new TcpListener(_testPort);
            _listener.NewClientEntered += client => _connectedClient = client;
            _listener.Start();
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
            using (var client = new TcpClient(IPAddress.Any, _testPort))
            {
                new Action(() => client.Connect()).Should().Throw<Exception>();
            }
        }

        [Test]
        public void Connect()
        {
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var connectCheck = false;
                client.Connected += () => connectCheck = true;
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var connectCheck = false;
                client.Connected += () => connectCheck = true;
                client.IsConnected.Should().BeFalse();

                client.ConnectAsync();

                Waiting.Until(() => connectCheck);
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect()).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void Send()
        {
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                IStreamBufferReader streamBufferReader = null;
                var sendData = new byte[] { 123 };

                client.Connect();

                Waiting.Until(() => _connectedClient != null);
                _connectedClient.BytesReceived += buffer => streamBufferReader = buffer; 

                client.Send(sendData);

                Waiting.Until(() => streamBufferReader != null);
                streamBufferReader.Buffer.Take(1).Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void SendAsync()
        {
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                IStreamBufferReader streamBufferReader = null;
                var sendData = new byte[] { 234 };

                client.Connect();

                Waiting.Until(() => _connectedClient != null);
                _connectedClient.BytesReceived += buffer => streamBufferReader = buffer;

                client.SendAsync(sendData);

                Waiting.Until(() => streamBufferReader != null);
                streamBufferReader.Buffer.Take(1).Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void Disconnect()
        {
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;
                
                client.Connect();
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
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
            using (var client = new TcpClient(IPAddress.Loopback, _testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
                client.Dispose();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.Dispose()).Should().NotThrow();
            }
        }
    }
}
