using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RunThis.Core.Invoker;

namespace RunThis.Tests.Targets
{
    public class FighterProxy : IFighter<bool>
    {
        private readonly IFighter<bool> _target;
        private readonly IInvoker _invoker;


        public FighterProxy(IFighter<bool> target, IInvoker invoker)
        {
            _target = target;
            _invoker = invoker;
        }

        readonly struct GetReadyCall : ICall
        {
            private readonly IFighter<bool> _target;
            public GetReadyCall(IFighter<bool> target) => _target = target;
            public ValueTask Invoke() => _target.GetReady();
        }

        readonly struct GetRemainingHealthCall : ICall<int>
        {
            private readonly IFighter<bool> _target;
            public GetRemainingHealthCall(IFighter<bool> target) => _target = target;
            public ValueTask<int> Invoke() => _target.GetRemainingHealth();
        }

        readonly struct TakeDamageCall : ICall
        {
            private readonly IFighter<bool> _target;
            private readonly int _value;

            public TakeDamageCall(IFighter<bool> target, int value)
            {
                _target = target;
                _value = value;
            }

            public ValueTask Invoke() => _target.TakeDamage(_value);
        }

        readonly struct CanTakeDamageCall : ICall<bool>
        {
            private readonly IFighter<bool> _target;
            private readonly int _value;

            public CanTakeDamageCall(IFighter<bool> target, int value)
            {
                _target = target;
                _value = value;
            }

            public ValueTask<bool> Invoke() => _target.CanTakeDamage(_value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask ExecuteVoidCall(ICall call)
        {
            return _invoker.ExecuteVoidCall(call);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<T> ExecuteValueCall<T>(ICall<T> call)
        {
            return _invoker.ExecuteValueCall(call);
        }

        public ValueTask GetReady()
        {
            return ExecuteVoidCall(new GetReadyCall(_target));
        }

        public ValueTask<int> GetRemainingHealth()
        {
            return ExecuteValueCall(new GetRemainingHealthCall(_target));
        }

        public ValueTask TakeDamage(int value)
        {
            return ExecuteVoidCall(new TakeDamageCall(_target, value));
        }

        public ValueTask<bool> CanTakeDamage(int value)
        {
            return ExecuteValueCall(new CanTakeDamageCall(_target, value));
        }
    }
}