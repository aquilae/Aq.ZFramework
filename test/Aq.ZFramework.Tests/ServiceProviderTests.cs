using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;
using NetMQ.Sockets;
using NUnit.Framework;

namespace Aq.ZFramework.Tests {
    [TestFixture, Parallelizable(ParallelScope.None)]
    public class ServiceProviderTests : UnitTests {
        [Test]
        public async Task ServerShouldCreateResolutionScopeForMiddleware() {
            var countingServiceFactory = new CountingServiceFactory();
            var ordinals = new List<int>(2);

            var server = new ServerBuilder()
                .UseServiceProvider(conf => {
                    conf.AddScoped(x => countingServiceFactory.CreateInstance());
                })
                .Use((ctx, next) => {
                    ordinals.Add(ctx.GetRequiredService<CountingService>().Ordinal);
                    return next();
                })
                .Use((ctx, next) => {
                    ordinals.Add(ctx.GetRequiredService<CountingService>().Ordinal);
                    return next();
                })
                .Bind("ipc://test")
                .BuildAndStart();

            try { 
                var serviceProvider = server.GetServiceProvider();
                var outer = serviceProvider.GetRequiredService<CountingService>();

                Assert.AreEqual(0, outer.Ordinal, "0 == outer.Ordinal");

                using (var client = new DealerSocket(">ipc://test")) {
                    client.SendFrameEmpty();

                    await Task.Delay(100);
                    Assert.AreEqual(2, countingServiceFactory.TotalCount, "2 == countingServiceFactory.TotalCount");

                    client.SendFrameEmpty();

                    await Task.Delay(100);
                    Assert.AreEqual(3, countingServiceFactory.TotalCount, "3 == countingServiceFactory.TotalCount");
                }

                Assert.AreEqual(4, ordinals.Count, "4 == ordinals.Count");
                Assert.AreEqual(1, ordinals[0], "1 == ordinals[0]");
                Assert.AreEqual(1, ordinals[1], "1 == ordinals[1]");
                Assert.AreEqual(2, ordinals[2], "2 == ordinals[2]");
                Assert.AreEqual(2, ordinals[3], "2 == ordinals[3]");
            }
            finally {
                server.Complete();
            }

            await server.Completion;
        }

        [Test]
        public async Task ServerShouldInjectMiddlewareDependencies() {
            var server = new ServerBuilder()
                .UseServiceProvider(conf => {
                    conf.AddSingleton<CountingServiceFactory>();
                    conf.AddTransient<DependantMiddleware>();
                })
                .Use<DependantMiddleware>()
                .Bind("ipc://test")
                .BuildAndStart();

            try {
                using (var client = new DealerSocket(">ipc://test")) {
                    client.SendFrameEmpty();
                    await Task.Delay(100);
                }

                var serviceProvider = server.GetServiceProvider();
                var countingServiceFactory = serviceProvider.GetRequiredService<CountingServiceFactory>();

                Assert.AreEqual(1, countingServiceFactory.TotalCount, "1 == countingServiceFactory.TotalCount");
            }
            finally {
                server.Complete();
            }

            await server.Completion;
        }

        private class CountingServiceFactory {
            public int TotalCount { get; private set; }

            public CountingService CreateInstance() {
                return new CountingService(this.TotalCount++);
            }
        }

        private class CountingService {
            public int Ordinal { get; }

            public CountingService(int ordinal) {
                this.Ordinal = ordinal;
            }
        }

        private class DependantMiddleware : IMiddleware {
            public CountingServiceFactory CountingServiceFactory { get; }

            public DependantMiddleware(CountingServiceFactory countingServiceFactory) {
                this.CountingServiceFactory = countingServiceFactory;
            }

            public Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
                this.CountingServiceFactory.CreateInstance();
                return next();
            }
        }
    }
}
