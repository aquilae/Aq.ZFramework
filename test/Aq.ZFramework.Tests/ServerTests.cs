using System.Threading.Tasks;
using Aq.ZFramework.Internal;
using NetMQ;
using NetMQ.Sockets;
using NUnit.Framework;

namespace Aq.ZFramework.Tests {
    [TestFixture, Parallelizable(ParallelScope.None)]
    public class ServerTests : UnitTests {
        [Test]
        public void ServerShouldStartInternalDispatcher() {
            var server = (Server) new ServerBuilder().BuildAndStart();
            Assert.That(server.RequestDispatcher.IsRunning);
            server.Complete();
        }

        [Test]
        public async Task ServerShouldInvokeMiddlewareUponReceivingRequest() {
            var tcs = new TaskCompletionSource();

            Middleware middleware = (ctx, next) => {
                tcs.SetComplete();
                return next();
            };

            var server = new ServerBuilder().Bind("ipc://test").Use(middleware).BuildAndStart();

            try {
                await Task.Delay(100);
                if (!server.Completion.IsPending()) {
                    if (server.Completion.IsCompleted) {
                        Assert.Fail("Server stopped");
                    }
                    else {
                        await server.Completion;
                    }
                }

                using (var client = new DealerSocket(">ipc://test")) {
                    client.SendFrame("Hello");

                    await Task.Delay(100);
                    Assert.That(tcs.Task.IsCompleted, "tcs.Task.IsCompleted");
                }
            }
            finally {
                server.Complete();
            }

            await server.Completion;
        }
    }
}
