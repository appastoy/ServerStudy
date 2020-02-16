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
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
            {
                var connectCheck = false;
                client.Connected += () => connectCheck = true;
                connectCheck.Should().BeFalse();
                client.IsConnected.Should().BeFalse();

                client.ConnectAsync();

                Task.Run(() => { while (connectCheck == false) Thread.Yield(); }).Wait();
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
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
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
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
            {
                var disconnectCheck = false;
                client.Disconnected += () => disconnectCheck = true;

                client.Connect();
                client.DisconnectAsync();

                Task.Run(() => { while (disconnectCheck == false) Thread.Yield(); }).Wait();
                disconnectCheck.Should().BeTrue();
                client.IsConnected.Should().BeFalse();
                client.IsDisposed.Should().BeFalse();
                new Action(() => client.DisconnectAsync()).Should().NotThrow();
            }
        }

        [Test]
        public void DisconnectAndCloseAync()
        {
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
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
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
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
            using (var client = new TcpClient(IPAddress.Loopback, testPort))
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
