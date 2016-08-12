using System;
using System.Threading.Tasks;

namespace Aq.ZFramework {
    /// <summary>
    /// Element of <see cref="IServer"/> pipeline
    /// </summary>
    public interface IMiddleware {
        Task ExecuteAsync(IMiddlewareContext context, Func<Task> next);
    }
}
