using System;
using Aq.ZFramework.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Aq.ZFramework {
    public static class ServerBuilderExtensions {
        /// <summary>
        /// Appends <see cref="middleware"/> to server pipeline
        /// </summary>
        /// <param name="self"></param>
        /// <param name="middleware"><see cref="IMiddleware.ExecuteAsync"/></param>
        /// <returns></returns>
        public static ServerBuilder Use(this ServerBuilder self, Middleware middleware) {
            if (middleware == null) {
                throw new ArgumentNullException(nameof (middleware));
            }
            
            return self.Use(new DelegateMiddleware(middleware));
        }

        /// <summary>
        /// Associates <see cref="IServiceProvider"/> with server
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buildContainer">Use to build <see cref="IServiceProvider"/>. Should return valid <see cref="IServiceProvider"/>
        /// built from provded <see cref="IServiceCollection"/></param>
        /// <returns></returns>
        public static ServerBuilder UseServiceProvider(
            this ServerBuilder self, Func<IServiceCollection, IServiceProvider> buildContainer) {

            if (buildContainer == null) {
                throw new ArgumentNullException(nameof (buildContainer));
            }

            var services = new ServiceCollection();
            var serviceProvider = buildContainer(services)
                ?? services.BuildServiceProvider();

            return self
                .Set(typeof (IServiceProvider), serviceProvider)
                .Use(new ServiceProviderMiddleware());
        }

        /// <summary>
        /// Associates <see cref="IServiceProvider"/> with server
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buildContainer">Use to build <see cref="IServiceProvider"/></param>
        /// <returns></returns>
        public static ServerBuilder UseServiceProvider(
            this ServerBuilder self, Action<IServiceCollection> buildContainer) {

            return self.UseServiceProvider(
                services => {
                    buildContainer(services);
                    return null;
                });
        }

        /// <summary>
        /// Associates <see cref="IServiceProvider"/> with server
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ServerBuilder UseServiceProvider(this ServerBuilder self) {
            return self.UseServiceProvider(services => null);
        }

        /// <summary>
        /// Appends middleware to server that will create new instance of <see cref="middlewareInstanceType"/>
        /// using associated <see cref="IServiceProvider"/> and call <see cref="IMiddleware.ExecuteAsync"/> on it
        /// </summary>
        /// <param name="self"></param>
        /// <param name="middlewareInstanceType"></param>
        /// <returns></returns>
        public static ServerBuilder Use(this ServerBuilder self, Type middlewareInstanceType) {
            return self.Use(new ResolvingMiddleware(middlewareInstanceType));
        }

        /// <summary>
        /// Appends middleware to server that will create new instance of <see cref="TMiddleware"/>
        /// using associated <see cref="IServiceProvider"/> and call <see cref="IMiddleware.ExecuteAsync"/> on it
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ServerBuilder Use<TMiddleware>(this ServerBuilder self) where TMiddleware : IMiddleware {
            return self.Use(new ResolvingMiddleware<TMiddleware>());
        }
    }
}
