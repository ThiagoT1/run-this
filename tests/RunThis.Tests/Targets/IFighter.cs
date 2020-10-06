using System.Threading.Tasks;

namespace RunThis.Tests.Targets
{
    public interface IFighter
    {
        ValueTask GetReady();

        ValueTask TakeDamage(int value);

        ValueTask<bool> CanTakeDamage(int value);

        ValueTask<int> GetRemainingHealth();

    }

    public class Fighter : IFighter
    {
        int _health;
        public ValueTask GetReady()
        {
            _health = 100;
            return default;
        }

        public ValueTask<int> GetRemainingHealth()
        {
            return new ValueTask<int>(_health);
        }

        public ValueTask TakeDamage(int value)
        {
            _health -= value;
            return default;
        }

        public ValueTask<bool> CanTakeDamage(int value)
        {
            return new ValueTask<bool>(_health - value >= 0);
        }
    }


}
