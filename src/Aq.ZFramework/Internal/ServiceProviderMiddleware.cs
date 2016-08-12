using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Aq.ZFramework.Internal {
    public class ServiceProviderMiddleware : IMiddleware {
        public ServiceProviderMiddleware() {
        }

        public async Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
            var serviceProviderType = typeof (IServiceProvider);

            var serviceProvider = (IServiceProvider) context.Server.Items[serviceProviderType];

            if (serviceProvider == null) {
                throw new Exception(
                    "No service provider found in server items." +
                    " Forgot .UseServiceProvider()?");
            }

            var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

            if (serviceScopeFactory == null) {
                context.Items[serviceProviderType] = serviceProvider;
                await next();
            }
            else {
                var serviceScope = serviceScopeFactory.CreateScope();

                try {
                    context.Items[serviceProviderType] = serviceScope.ServiceProvider;
                    await next();
                }
                finally {
                    serviceScope.Dispose();
                }
            }
        }
    }
}
