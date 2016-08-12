using System;
using System.Threading.Tasks;

namespace Aq.ZFramework.Internal {
    public class DelegateMiddleware : IMiddleware {
        // exposed for testing purposes
        public Middleware Execute { get; }

        public DelegateMiddleware(Middleware execute) {
            if (execute == null) {
                throw new ArgumentNullException(nameof (execute));
            }

            this.Execute = execute;
        }

        public Task ExecuteAsync(IMiddlewareContext context, Func<Task> next) {
            return this.Execute(context, next);
        }
    }
}
