using FluentAssertions;
using NUnit.Framework;
using System;

namespace Neti.Tests
{
    public class TcpListenerTests
    {
        TcpListener _listener;

        [SetUp]
        public void Setup()
        {
            _listener = new TcpListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener.Dispose();
        }

        [Test]
        public void Start()
        {
            _listener.IsActive.Should().BeFalse();
            _listener.Start();
            _listener.IsActive.Should().BeTrue();
            new Action(() => _listener.Start()).Should()
                                               .Throw<InvalidOperationException>()
                                               .WithMessage("Already started.");
        }

        [Test]
        public void Stop()
        {
            new Action(() => _listener.Stop()).Should().NotThrow();
            _listener.Start();
            _listener.Stop();
            _listener.IsActive.Should().BeFalse();
            new Action(() => _listener.Stop()).Should().NotThrow();
        }
    }
}