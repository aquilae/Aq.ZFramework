using System;
using System.Threading.Tasks;

namespace Aq.ZFramework.Internal {
    public class ResolvingMiddleware : IMiddleware {
        public ResolvingMiddleware(Type instanceType) {
            this.InstanceType = instanceType;
        }

        public Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
            var instance = context.GetRequiredService(this.InstanceType);
            return ((IMiddleware) instance).ExecuteAsync(context, next);
        }

        private Type InstanceType { get; }
    }

    public class ResolvingMiddleware<TInstance> : ResolvingMiddleware where TInstance : IMiddleware {
        public ResolvingMiddleware() : base(typeof (TInstance)) {
        }
    }
}
