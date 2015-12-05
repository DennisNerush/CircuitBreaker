using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace CircuitBreaker
{
    public class NavigationHistoryController
    {
        private readonly INavigationHistoryProvider _historyProvider;
        private Policy _policy;
        private const int TimeoutExceptionCode = -2;

        public NavigationHistoryController(INavigationHistoryProvider historyProvider)
        {
            _historyProvider = historyProvider;
            _policy = Policy.Handle<SqlException>(ex => ex.Number == TimeoutExceptionCode).CircuitBreaker(10, TimeSpan.FromMinutes(10));
        }

        public List<History> GetHistory()
        {
            try
            {
                return _policy.Execute(() => _historyProvider.Get());
            }
            catch (SqlException exception)
            {
                return null;
            }
            catch (BrokenCircuitException)
            {
                return new List<History>();
            }
        }
    }
}
