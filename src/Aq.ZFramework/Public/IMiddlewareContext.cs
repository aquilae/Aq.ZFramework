using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace Aq.ZFramework {
    public interface IMiddlewareContext {
        /// <summary>
        /// <see cref="IServer"/> that called this middleware
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// Internally <see cref="IServer"/> uses Router sockets. See ZeroMQ documentation
        /// </summary>
        byte[] ClientPrefix { get; }

        /// <summary>
        /// Frames of ZeroMQ message that triggered this request
        /// </summary>
        NetMQMessage Request { get; }

        /// <summary>
        /// Cancels when <see cref="IServer"/> tries to stop
        /// </summary>
        CancellationToken Cancellation { get; }

        /// <summary>
        /// Arbitrary items associated with current request. Use to share information between middleware
        /// </summary>
        IDictionary<object, object> Items { get; }
            
        /// <summary>
        /// Sends data back to client that sent this request
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task ReplyAsync(
            ICollection<NetMQFrame> frames,
            CancellationToken cancellation
                = default (CancellationToken));
    }
}
