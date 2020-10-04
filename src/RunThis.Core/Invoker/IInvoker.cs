using System.Threading.Tasks;

namespace RunThis.Core.Invoker
{
    public interface IInvoker
    {
        int QueueSize { get; }

        ValueTask<T> ExecuteValueCall<T>(ICall<T> call);

        ValueTask ExecuteVoidCall(ICall call);
    }


}
