using System;

namespace Aq.ZFramework {
    public static class ServerExtensions {
        /// <summary>
        /// Returns <see cref="IServiceProvider"/> associated with this server.
        /// See <see cref="ServerBuilderExtensions.UseServiceProvider(ServerBuilder)"/>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(this IServer self) {
            return (IServiceProvider) self.Items[typeof (IServiceProvider)];
        }
    }
}
