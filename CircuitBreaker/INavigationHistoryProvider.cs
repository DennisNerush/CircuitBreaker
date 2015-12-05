using System.Collections.Generic;

namespace CircuitBreaker
{
    public interface INavigationHistoryProvider
    {
        List<History>Get();
    }
}