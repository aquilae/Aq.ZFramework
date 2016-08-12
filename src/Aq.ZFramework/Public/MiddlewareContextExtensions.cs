using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;

namespace Aq.ZFramework {
    public static class MiddlewareContextExtensions {
        /// <summary>
        /// Sends data back to client that sent this request
        /// </summary>
        /// <param name="self"></param>
        /// <param name="encoding"></param>
        /// <param name="firstFrame"></param>
        /// <param name="otherFrames"></param>
        /// <returns></returns>
        public static Task ReplyAsync(
            this IMiddlewareContext self, Encoding encoding,
            string firstFrame, params string[] otherFrames) {

            int frameCount = 1;
            if (otherFrames != null) {
                frameCount = otherFrames.Length + 1;
            }

            var frames = new List<NetMQFrame>(frameCount) {
                new NetMQFrame(firstFrame, encoding)
            };

            if (frameCount > 1) {
                // ReSharper disable once PossibleNullReferenceException
                foreach (var frame in otherFrames) {
                    frames.Add(new NetMQFrame(frame, encoding));
                }
            }

            return self.ReplyAsync(frames);
        }

        /// <summary>
        /// Sends data back to client that sent this request
        /// </summary>
        /// <param name="self"></param>
        /// <param name="firstFrame"></param>
        /// <param name="otherFrames"></param>
        /// <returns></returns>
        public static Task ReplyAsync(
            this IMiddlewareContext self,
            string firstFrame, params string[] otherFrames) {

            return self.ReplyAsync(Encoding.UTF8, firstFrame, otherFrames);
        }

        /// <summary>
        /// Returns <see cref="IServiceProvider"/> associated with this request.
        /// See <see cref="ServerBuilderExtensions.UseServiceProvider(ServerBuilder)"/>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(this IMiddlewareContext self) {
            return (IServiceProvider) self.Items[typeof (IServiceProvider)];
        }

        /// <summary>
        /// See <see cref="IServiceProvider.GetService(Type)"/>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="serviceType"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static object GetService(this IMiddlewareContext self, Type serviceType) {
            return self.GetRequiredServiceProvider().GetService(serviceType);
        }

        /// <summary>
        /// See <see cref="ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider, Type)"/>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="serviceType"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static object GetRequiredService(this IMiddlewareContext self, Type serviceType) {
            return self.GetRequiredServiceProvider().GetRequiredService(serviceType);
        }

        /// <summary>
        /// See <see cref="ServiceProviderServiceExtensions.GetServices(IServiceProvider, Type)"/>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="serviceType"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static IEnumerable<object> GetServices(this IMiddlewareContext self, Type serviceType) {
            return self.GetRequiredServiceProvider().GetServices(serviceType);
        }

        /// <summary>
        /// See <see cref="ServiceProviderServiceExtensions.GetService&lt;T&gt;(IServiceProvider)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static T GetService<T>(this IMiddlewareContext self) {
            return self.GetRequiredServiceProvider().GetService<T>();
        }

        /// <summary>
        /// See <see cref="ServiceProviderServiceExtensions.GetRequiredService&lt;T&gt;(IServiceProvider)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static T GetRequiredService<T>(this IMiddlewareContext self) {
            return self.GetRequiredServiceProvider().GetRequiredService<T>();
        }

        /// <summary>
        /// See <see cref="ServiceProviderServiceExtensions.GetServices&lt;T&gt;(IServiceProvider)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <exception cref="Exception">No <see cref="IServiceProvider"/> was set</exception>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>(this IMiddlewareContext self) {
            return self.GetRequiredServiceProvider().GetServices<T>();
        }

        private static IServiceProvider GetRequiredServiceProvider(this IMiddlewareContext self) {
            var serviceProvider = self.GetServiceProvider();

            if (serviceProvider == null) {
                throw new Exception(
                    "No service provider found in middleware items." +
                    " Forgot .UseServiceProvider()?");
            }

            return serviceProvider;
        
        }
    }
}
