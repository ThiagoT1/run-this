using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RunThis.Core.Invoker
{

    public class SingleThreadInvoker : BaseInvoker
    {
        private readonly struct ExecutionSlot { }
        private readonly ExecutionSlot Slot;

        private readonly Channel<ExecutionSlot> _callChannel;
        private readonly ChannelReader<ExecutionSlot> _channelReader;
        private readonly ChannelWriter<ExecutionSlot> _channelWriter;

        public SingleThreadInvoker()
        {
            Slot = new ExecutionSlot();

            _callChannel = Channel.CreateUnbounded<ExecutionSlot>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = false,
                SingleWriter = true
            });

            _channelReader = _callChannel.Reader;
            _channelWriter = _callChannel.Writer;

            ReturnExecutionSlot();
        }

        public override async ValueTask<T> ExecuteValueCall<T>(ICall<T> call)
        {
            if (!ClaimExecutionSlot())
                await WaitForExecutionSlot();

            try
            {
                return await call.Invoke();
            }
            finally
            {
                ReturnExecutionSlot();
            }
        }

        public override async ValueTask ExecuteVoidCall(ICall call)
        {
            if (!ClaimExecutionSlot())
                await WaitForExecutionSlot();

            try
            {
                await call.Invoke();
            }
            finally
            {
                ReturnExecutionSlot();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask WaitForExecutionSlot()
        {
            IncreaseQueue();
            _ = await _channelReader.ReadAsync();
            DecreaseQueue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ClaimExecutionSlot() => _channelReader.TryRead(out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReturnExecutionSlot() => _channelWriter.TryWrite(Slot);

    }


}
