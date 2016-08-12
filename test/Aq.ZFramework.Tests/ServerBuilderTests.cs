using System;
using System.Linq;
using Aq.ZFramework.Internal;
using NUnit.Framework;

namespace Aq.ZFramework.Tests {
    [TestFixture, Parallelizable(ParallelScope.None)]
    public class ServerBuilderTests : UnitTests {
        [Test]
        public void ServerBuilderAddShouldThrowWhenAddingSameKeyTwice() {
            var key = new object();
            var serverBuilder = new ServerBuilder();
            serverBuilder.Add(key, null);
            Assert.Throws<ArgumentException>(() => serverBuilder.Add(key, null));
        }

        [Test]
        public void ServerBuilderSetShouldOverwriteWhenAddingSameKeyTwice() {
            var key = new object();
            var value1 = new object();
            var value2 = new object();
            var serverBuilder = new ServerBuilder();
            serverBuilder.Set(key, value1);
            serverBuilder.Set(key, value2);
            var server = serverBuilder.BuildAndStart();
            server.Complete();
            Assert.AreEqual(value2, server.Items[key]);
        }

        [Test]
        public void ServerBuilderRemoveShouldNotFailWhenRemovingNonExistingKey() {
            var serverBuilder = new ServerBuilder();
            serverBuilder.Remove(new object());
        }

        [Test]
        public void ServerBuilderShouldPassMiddlewareToServer() {
            Middleware middleware = (ctx, next) => next();

            var server = (Server) new ServerBuilder()
                .Use(middleware).BuildAndStart();

            server.Complete();
            Assert.IsInstanceOf<DelegateMiddleware>(server.Middleware.First(), "server.Middleware.First() is DelegateMiddleware");
            Assert.AreEqual(middleware, ((DelegateMiddleware) server.Middleware.First()).Execute);
        }

        [Test]
        public void ServerBuilderShouldStartBuiltServer() {
            Middleware middleware = (ctx, next) => next();

            var server = (Server) new ServerBuilder()
                .Use(middleware).BuildAndStart();

            Assert.That(server.IsRunning);

            server.Complete();
        }
    }
}
