using System;
using System.Threading.Tasks;

namespace Aq.ZFramework {
    public delegate Task Middleware(IMiddlewareContext context, Func<Task> next);
}
