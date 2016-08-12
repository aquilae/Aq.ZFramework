using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aq.NetMQ;
using NetMQ;
using NetMQ.Sockets;

namespace Aq.ZFramework.Internal {
    public class Server : IServer {
        public Task Completion { get; }
        public IReadOnlyList<string> Endpoints { get; }
        public IDictionary<object, object> Items { get; }
        public bool IsRunning => this.RequestDispatcher.IsRunning;

        // exposed for testing purposes
        public ICollection<IMiddleware> Middleware { get; }
        public RequestDispatcher RequestDispatcher { get; }

        public Server(
            ICollection<string> endpoints,
            ICollection<string> addresses,
            IDictionary<object, object> items,
            ICollection<IMiddleware> middleware) {
            
            const TaskContinuationOptions taskContinuationOptions
                = TaskContinuationOptions.ExecuteSynchronously
                | TaskContinuationOptions.DenyChildAttach;

            var resultingEndpoints = new List<string>(endpoints.Count + addresses.Count);
            this.Sockets = new Dictionary<string, NetMQSocket>(endpoints.Count + addresses.Count);

            try {
                foreach (var endpoint in endpoints) {
                    var socket = new RouterSocket();
                    this.Sockets.Add(endpoint, socket);
                    socket.Bind(endpoint);
                    resultingEndpoints.Add(endpoint);
                }

                foreach (var address in addresses) {
                    var socket = new RouterSocket();
                    this.Sockets.Add(address, socket);
                    var port = socket.BindRandomPort(address);
                    this.Sockets.Add(address + ":" + port, socket);
                    this.Sockets.Remove(address);
                    resultingEndpoints.Add(address + ":" + port);
                }

                this.Endpoints = resultingEndpoints;

                this.Middleware = middleware;
                this.Items = new ConcurrentDictionary<object, object>(items);

                this.RequestDispatcher = new RequestDispatcher(this.HandleRequestAsync);

                foreach (var socket in this.Sockets.Values) {
                    this.RequestDispatcher.Add(socket);
                }

                this.RunningRequestDispatcher = this.RequestDispatcher.Start();

                this.Completion = this.RequestDispatcher.Running.Completion;
                this.Completion.ContinueWith(this.CompletionContinuation, taskContinuationOptions);
            }
            catch {
                foreach (var kv in this.Sockets) {
                    try {
                        kv.Value.Unbind(kv.Key);
                    }
                    catch {
                        // ignore
                    }
                    kv.Value.Close();
                    kv.Value.Dispose();
                }
                throw;
            }
        }

        private void CompletionContinuation(Task task) {
            this.RequestDispatcher.Dispose();
            foreach (var kv in this.Sockets) {
                try {
                    kv.Value.Unbind(kv.Key);
                }
                catch {
                    // ignore
                }
                kv.Value.Close();
                kv.Value.Dispose();
            }
        }

        public void Complete() {
            this.RunningRequestDispatcher.Complete();
        }

        private IDictionary<string, NetMQSocket> Sockets { get; }
        private IRunningRequestDispatcher RunningRequestDispatcher { get; }

        private async Task HandleRequestAsync(IRequestHandlerContext context) {
            using (var middlewareContext = new MiddlewareContext(
                this, context, this.Middleware.GetEnumerator())) {

                await middlewareContext.Next();
            }
        }
    }
}
