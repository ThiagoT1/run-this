using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RunThis.Tests
{
    public static class IdProvider
    {
        static int _nextId;
        public static int GetNextId()
        {
            int result = 0;

            unchecked {
                result = Interlocked.Increment(ref _nextId);
            }

            return result;
        }
    }

    public interface ICall<T>
    {
        ValueTask<T> Invoke();

    }

    public interface ICall
    {
        ValueTask Invoke();

    }


    public abstract class Proxy
    {

        public const int Idle = 0;
        public const int RunningOne = 1;
        public const int Scheduling = 1;
        public const int RunningLate = 2;

        int _state;
        int _queueCount;

        readonly struct ExecutionSlot { }
        readonly ExecutionSlot Slot;

        private readonly Channel<ExecutionSlot> _callChannel;
        private readonly ILogger _logger;
        public Proxy(ILogger logger)
        {
            _logger = logger;

            Slot = new ExecutionSlot();

            _callChannel = Channel.CreateUnbounded<ExecutionSlot>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = false,
                SingleWriter = true
            });

            _callChannel.Writer.TryWrite(Slot);

        }

        private void IncreaseQueue()
        {
            Interlocked.Increment(ref _queueCount);
        }
        private void DecreaseQueue()
        {
            Interlocked.Increment(ref _queueCount);
        }

        public int QueueSize => _queueCount;

        protected async ValueTask<T> ExecuteValueCall<T>(ICall<T> call)
        {
            

            if (!_callChannel.Reader.TryRead(out _))
            {
                IncreaseQueue();
                await _callChannel.Reader.ReadAsync();
                DecreaseQueue();
            }

            try
            {
                return await call.Invoke();
            }
            finally
            {
                _callChannel.Writer.TryWrite(Slot);
            }
        }

        protected async ValueTask ExecuteVoidCall(ICall call)
        {

            if (!_callChannel.Reader.TryRead(out _))
            {
                IncreaseQueue();
                await _callChannel.Reader.ReadAsync();
                DecreaseQueue();
            }

            try
            {
                await call.Invoke();
            }
            finally
            {        
                _callChannel.Writer.TryWrite(Slot);
            }
        }

    }


}
