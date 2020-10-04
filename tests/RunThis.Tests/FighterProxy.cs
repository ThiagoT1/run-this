using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RunThis.Tests
{
    public class FighterProxy : Proxy, IFighter
    {
        private readonly IFighter _target;
        
        public FighterProxy(IFighter target, ILoggerFactory loggerFactory)
            : base (loggerFactory.CreateLogger<FighterProxy>())
        {
            _target = target;
        }

        readonly struct GetReadyCall : ICall
        {
            private readonly IFighter _target;
            public GetReadyCall(IFighter target) => _target = target;
            public ValueTask Invoke() => _target.GetReady();
        }

        readonly struct GetRemainingHealthCall : ICall<int>
        {
            private readonly IFighter _target;
            public GetRemainingHealthCall(IFighter target) => _target = target;
            public ValueTask<int> Invoke() => _target.GetRemainingHealth();
        }

        readonly struct TakeDamageCall : ICall
        {
            private readonly IFighter _target;
            private readonly int _value;

            public TakeDamageCall(IFighter target, int value)
            {
                _target = target;
                _value = value;
            }

            public ValueTask Invoke() => _target.TakeDamage(_value);
        }

        public ValueTask GetReady() => ExecuteVoidCall(new GetReadyCall(_target));

        public ValueTask<int> GetRemainingHealth() => ExecuteValueCall(new GetRemainingHealthCall(_target));
        
        public ValueTask TakeDamage(int value) => ExecuteVoidCall(new TakeDamageCall(_target, value));
        
    }

}
