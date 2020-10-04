using System.Threading;
using System.Threading.Tasks;

namespace RunThis.Core.Invoker
{
    public abstract class BaseInvoker : IInvoker
    {
        

        int _queueCount;
        protected void IncreaseQueue()
        {
            Interlocked.Increment(ref _queueCount);
        }
        protected void DecreaseQueue()
        {
            Interlocked.Increment(ref _queueCount);
        }

        public int QueueSize => Volatile.Read(ref _queueCount);

        public abstract ValueTask<T> ExecuteValueCall<T>(ICall<T> call);

        public abstract ValueTask ExecuteVoidCall(ICall call);

    }


}
