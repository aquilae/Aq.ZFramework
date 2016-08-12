using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aq.ZFramework {
    public interface IServer {
        /// <summary>
        /// True if the server is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Will complete or fail upon server stop
        /// </summary>
        Task Completion { get; }

        /// <summary>
        /// List of addresses that the server is bound to. Useful when using <see cref="ServerBuilder.BindRandomPort"/>
        /// </summary>
        IReadOnlyList<string> Endpoints { get; }

        /// <summary>
        /// Arbitrary items associated with this server instance
        /// </summary>
        IDictionary<object, object> Items { get; }

        /// <summary>
        /// Tells server to launch stop process. Server will cease to accept incoming requests,
        /// wait for pending handlers to complete, unbind and close used sockets and complete <see cref="IServer.Completion"/>
        /// </summary>
        void Complete();
    }
}
