using System.Threading.Tasks;

namespace RunThis.Core.Invoker
{

    public interface ICall
    {
        ValueTask Invoke();
    }

    public interface ICall<T>
    {
        ValueTask<T> Invoke();
    }


}
