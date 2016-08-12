using System.Threading.Tasks;

namespace Aq.ZFramework.Tests {
    internal class TaskCompletionSource : TaskCompletionSource<byte> {
        public void SetComplete() => this.SetResult(0);
        public bool TrySetComplete() => this.TrySetResult(0);
    }
}
