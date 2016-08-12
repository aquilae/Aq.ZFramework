using System;
using System.Collections.Generic;
using Aq.ZFramework.Internal;

namespace Aq.ZFramework {
    public class ServerBuilder {
        public ServerBuilder() {
            this.Endpoints = new List<string>();
            this.Addresses = new List<string>();
            this.Middleware = new List<IMiddleware>();
            this.Items = new Dictionary<object, object>();
        }

        /// <summary>
        /// Instructs builder to bind created server to specific endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint address in ZeroMQ supported format (e.g. "tcp://127.0.0.1:5555")</param>
        /// <returns></returns>
        public ServerBuilder Bind(string endpoint) {
            this.Endpoints.Add(endpoint);
            return this;
        }

        /// <summary>
        /// Instructs builder to bind created server to address using random port
        /// </summary>
        /// <param name="address">Endpoint address in ZeroMQ supported format (e.g. "tcp://127.0.0.1")</param>
        /// <returns></returns>
        public ServerBuilder BindRandomPort(string address) {
            this.Addresses.Add(address);
            return this;
        }

        /// <summary>
        /// Adds new item to <see cref="IServer.Items"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ServerBuilder Add(object key, object value) {
            this.Items.Add(key, value);
            return this;
        }

        /// <summary>
        /// Adds or updates item in <see cref="IServer.Items"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ServerBuilder Set(object key, object value) {
            this.Items[key] = value;
            return this;
        }

        /// <summary>
        /// Removes item from <see cref="IServer.Items"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ServerBuilder Remove(object key) {
            this.Items.Remove(key);
            return this;
        }

        /// <summary>
        /// Adds middleware to server pipeline
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public ServerBuilder Use(IMiddleware middleware) {
            if (middleware == null) {
                throw new ArgumentNullException(nameof (middleware));
            }

            this.Middleware.Add(middleware);
            return this;
        }

        /// <summary>
        /// Builds and asynchronously runs a server
        /// </summary>
        /// <returns></returns>
        public IServer BuildAndStart() {
            return new Server(
                this.Endpoints, this.Addresses,
                this.Items, this.Middleware);
        }

        private List<string> Endpoints { get; }
        private List<string> Addresses { get; }
        private List<IMiddleware> Middleware { get; }
        private IDictionary<object, object> Items { get; }
    }
}
