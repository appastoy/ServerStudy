using FluentAssertions;
using NUnit.Framework;
using System;

namespace Neti.Tests
{
    public class TcpServerTests
    {
        TcpServer listener;

        [SetUp]
        public void Setup()
        {
            listener = new TcpServer();
        }

        [TearDown]
        public void TearDown()
        {
            listener.Dispose();
            listener = null;
        }

        [Test]
        public void Start()
        {
            listener.IsActive.Should().BeFalse();
            listener.Start();
            listener.IsActive.Should().BeTrue();
            new Action(() => listener.Start()).Should()
                                               .Throw<InvalidOperationException>()
                                               .WithMessage("Already started.");
        }

        [Test]
        public void Stop()
        {
            new Action(() => listener.Stop()).Should().NotThrow();
            listener.Start();
            listener.Stop();
            listener.IsActive.Should().BeFalse();
            new Action(() => listener.Stop()).Should().NotThrow();
        }
    }
}