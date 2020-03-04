using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Neti.Buffer;
using Neti.Packets;
using NUnit.Framework;

namespace Neti.Tests
{
    class TcpClientTests
    {
        const int testPort = 41717;

        TcpServer listener;
        TcpClient connectedSession;

        [SetUp]
        public void Setup()
        {
            listener = new TcpServer();
            listener.SessionEntered += session => connectedSession = session;
            listener.Start(testPort);
        }

        [TearDown]
        public void TearDown()
        {
            connectedSession?.Dispose();
            connectedSession = null;
            listener.Stop(); 
            listener.Dispose();
            listener = null;
        }

        [Test]
        public void ConnectToWrongAddress()
        {
            using (var client = new TcpClient())
            {
                new Action(() => client.Connect(IPAddress.Any, testPort)).Should().Throw<Exception>();
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

                client.Connect(IPAddress.Loopback, testPort);

                connectCheck.Should().BeTrue();
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect(IPAddress.Loopback, testPort)).Should().Throw<InvalidOperationException>();
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

                client.ConnectAsync(IPAddress.Loopback, testPort);

                Waiting.Until(() => connectCheck);
                client.IsConnected.Should().BeTrue();
                new Action(() => client.Connect(IPAddress.Loopback, testPort)).Should().Throw<InvalidOperationException>();
            }
        }

        [Test]
        public void Send()
        {
            using (var client = new TcpClient())
            {
                var bytes = new ArraySegment<byte>();
                var sendData = new byte[] { 123 };

                client.Connect(IPAddress.Loopback, testPort);

                Waiting.Until(() => connectedSession != null);
                connectedSession.PacketReceived += reader => bytes = reader.ReadBytes();

                client.Send(sendData);

                Waiting.Until(() => bytes.Array != null);
                bytes.Array.Skip(bytes.Offset).Take(bytes.Count).Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void SendAsync()
        {
            using (var client = new TcpClient())
            {
                ArraySegment<byte> bytes = null;
                var sendData = new byte[] { 234 };

                client.Connect(IPAddress.Loopback, testPort);

                Waiting.Until(() => connectedSession != null);
                connectedSession.PacketReceived += reader => bytes = reader.ReadBytes();

                client.SendAsync(sendData);

                Waiting.Until(() => bytes.Array != null);
                bytes.Array.Skip(bytes.Offset).Take(bytes.Count).Should().BeEquivalentTo(sendData);
            }
        }

        [Test]
        public void Disconnect()
        {
            using (var client = new TcpClient())
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect(IPAddress.Loopback, testPort);
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

                client.Connect(IPAddress.Loopback, testPort);
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

                client.Connect(IPAddress.Loopback, testPort);
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
                
                client.Connect(IPAddress.Loopback, testPort);
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

                client.Connect(IPAddress.Loopback, testPort);
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

                client.Connect(IPAddress.Loopback, testPort);
                client.Dispose();

                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeTrue();
                new Action(() => client.Dispose()).Should().NotThrow();
            }
        }
    }
}
