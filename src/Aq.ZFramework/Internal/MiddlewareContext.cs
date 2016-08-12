using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aq.NetMQ;
using NetMQ;

namespace Aq.ZFramework.Internal {
    public class MiddlewareContext : IDisposable, IMiddlewareContext {
        public IServer Server { get; }
        public byte[] ClientPrefix { get; }
        public IDictionary<object, object> Items { get; }
        public NetMQMessage Request => this.RequestHandlerContext.Request;
        public CancellationToken Cancellation => this.RequestHandlerContext.Cancellation;

        public MiddlewareContext(
            IServer server,
            IRequestHandlerContext requestHandlerContext,
            IEnumerator<IMiddleware> middlewareEnumerator) {

            this.Server = server;
            this.Items = new Dictionary<object, object>();
            
            this.MiddlewareEnumerator = middlewareEnumerator;
            this.RequestHandlerContext = requestHandlerContext;

            this.ClientPrefixFrame = requestHandlerContext.Request.Pop();
            this.ClientPrefix = this.ClientPrefixFrame.ToByteArray(true);
        }

        public void Dispose() {
            this.MiddlewareEnumerator.Dispose();
        }
        
        public Task ReplyAsync(ICollection<NetMQFrame> frames, CancellationToken cancellation = default (CancellationToken)) {
            cancellation.ThrowIfCancellationRequested();

            if (frames == null) {
                throw new ArgumentNullException(nameof (frames));
            }
            if (frames.Count == 0) {
                throw new ArgumentOutOfRangeException(
                    nameof (frames), "frames can not be empty");
            }

            var message = new NetMQMessage(frames.Count + 1);
            message.Append(this.ClientPrefixFrame);
            foreach (var frame in frames) {
                message.Append(frame);
            }

            return this.RequestHandlerContext.SendAsync(message, cancellation);
        }

        public Task Next() {
            if (this.MiddlewareEnumerator.MoveNext()) {
                return this.MiddlewareEnumerator.Current.ExecuteAsync(this, this.Next);
            }
            return CompletedTask;
        }

        private static readonly Task CompletedTask = Task.FromResult<byte>(0);

        private IEnumerator<IMiddleware> MiddlewareEnumerator { get; }
        private IRequestHandlerContext RequestHandlerContext { get; }
        private NetMQFrame ClientPrefixFrame { get; }
    }
}
