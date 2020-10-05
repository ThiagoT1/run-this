using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;

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

        public override ValueTask<T> ExecuteValueCall<T>(ICall<T> call)
        {
            if (!ClaimExecutionSlot())
                return SlowExecuteValueCall(this, call);

            if (FastExecuteValueCall(this, call, out var valueTask))
            {
                ReturnExecutionSlot();
                return valueTask;
            }

            return SlowAwait(this, valueTask);

            static async ValueTask<T> SlowExecuteValueCall(SingleThreadInvoker @this, ICall<T> call)
            {
                await @this.WaitForExecutionSlot();

                try
                {
                    return await call.Invoke();
                }
                finally
                {
                    @this.ReturnExecutionSlot();
                }
            }

            static async ValueTask<T> SlowAwait(SingleThreadInvoker @this, ValueTask<T> valueTask)
            {
                try
                {
                    return await valueTask;
                }
                finally
                {
                    @this.ReturnExecutionSlot();
                }
            }
            static bool FastExecuteValueCall(SingleThreadInvoker @this, ICall<T> call, out ValueTask<T> valueTask)
            {
                try
                {
                    valueTask = call.Invoke();
                }
                catch
                {
                    @this.ReturnExecutionSlot();
                    throw;
                }

                return valueTask.IsCompleted;
            }
        }

        public override ValueTask ExecuteVoidCall(ICall call)
        {
            if (!ClaimExecutionSlot())
                return SlowExecuteVoidCall(this, call);

            if (FastExecuteVoidCall(this, call, out var valueTask))
            {
                ReturnExecutionSlot();
                return valueTask;
            }

            return SlowAwait(this, valueTask);

            static async ValueTask SlowExecuteVoidCall(SingleThreadInvoker @this, ICall call)
            {
                await @this.WaitForExecutionSlot();

                try
                {
                    await call.Invoke();
                }
                finally
                {
                    @this.ReturnExecutionSlot();
                }
            }

            static async ValueTask SlowAwait(SingleThreadInvoker @this, ValueTask valueTask)
            {
                try
                {
                    await valueTask;
                }
                finally
                {
                    @this.ReturnExecutionSlot();
                }
            }
            static bool FastExecuteVoidCall(SingleThreadInvoker @this, ICall call, out ValueTask valueTask)
            {
                try
                {
                    valueTask = call.Invoke();
                }
                catch
                {
                    @this.ReturnExecutionSlot();
                    throw;
                }

                return valueTask.IsCompleted;
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
