using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;
using NetMQ.Sockets;

namespace Aq.ZFramework.Showcase {
    public class Program {
        static IContainer SetupAutofacContainer(IServiceCollection serviceCollection = null) {
            var conf = new ContainerBuilder();

            conf.Populate(serviceCollection ?? Enumerable.Empty<ServiceDescriptor>());

            conf.RegisterType<Middleware>()
                .InstancePerLifetimeScope().ExternallyOwned()
                .AsSelf();

            conf.RegisterType<ServiceFactory>()
                .SingleInstance().ExternallyOwned()
                .AsSelf();

            conf.Register(x => x.Resolve<ServiceFactory>().CreateInstance())
                .InstancePerLifetimeScope().ExternallyOwned()
                .As<IService>();

            return conf.Build();
        }

        static IServiceProvider SetupAutofacServiceProvider(IServiceCollection serviceCollection) {
            return new AutofacServiceProvider(SetupAutofacContainer(serviceCollection));
        }

        public static void Main(string[] args) {
            MainAsync().Wait();
            return;

            var server = new ServerBuilder()
                .Bind("ipc://showcase")
                .UseServiceProvider(SetupAutofacServiceProvider)
                .Use(async (ctx, next) => {
                    Console.WriteLine($"server received: {ctx.Request.First.ConvertToString()}");

                    try {
                        await next();
                    }
                    catch (Exception exc) {
                        Console.WriteLine("server faulted:");
                        Console.WriteLine(exc);
                    }
                })
                .Use<Middleware>()
                .BuildAndStart();

            try {
                var serviceFactory = server.GetServiceProvider().GetRequiredService<ServiceFactory>();
                using (var client = new DealerSocket(">ipc://showcase")) {
                    for (var i = 0; i < 5; ++i) {
                        client.SendFrame("Hey");
                        Console.WriteLine($"client received: {client.ReceiveFrameString()}");
                        Console.WriteLine($"serviceFactory.TotalCount: {serviceFactory.TotalCount}");
                    }
                    client.Close();
                }
            }
            finally {
                server.Complete();
            }

            server.Completion.Wait();
        }

  class InnerMiddleware : IMiddleware {
    public async Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
      await context.ReplyAsync("I am World!");
    }
  }

  static async Task MainAsync() {
    var server = new ServerBuilder()
      .Bind("tcp://127.0.0.1:8080")
      .UseServiceProvider(services => {
          services.AddTransient<InnerMiddleware>();
      })
      .Use(async (ctx, next) => {
        Debug.Assert("Hey" == ctx.Request.First.ConvertToString());
        await ctx.ReplyAsync("Hello");
        await next();
      })
      .Use<InnerMiddleware>()
      .BuildAndStart();

    using (var client = new DealerSocket(">tcp://127.0.0.1:8080")) {
      client.SendFrame("Hey");
      Debug.Assert("Hello" == client.ReceiveFrameString());
      Debug.Assert("I am World!" == client.ReceiveFrameString());
      client.Close();
    }

    server.Complete();
    await server.Completion;
  }
    }

    interface IService {
        int Ordinal { get; }
    }

    class Service : IService {
        public int Ordinal { get; }

        public Service(int ordinal) {
            Console.WriteLine("Service({0})", ordinal);
            this.Ordinal = ordinal;
        }
    }

    class ServiceFactory {
        public int TotalCount => this._totalCount;

        public ServiceFactory() {
            Console.WriteLine("ServiceFactory() #{0}", this.GetHashCode());
        }

        public IService CreateInstance() {
            Console.WriteLine("ServiceFactory.CreateInstance() #{0}", this.GetHashCode());
            return new Service(
                Interlocked.Increment(
                    ref this._totalCount) - 1);
        }

        private int _totalCount;
    }

    class Middleware : IMiddleware {
        public Middleware(IService service) {
            this.Service = service;
        }

        public async Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
            await context.ReplyAsync($"Hello #{this.Service.Ordinal}");
            await next();
        }

        private IService Service { get; }
    }
}
