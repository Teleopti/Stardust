using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class TraceableRefreshService : ITraceableRefreshService
    {
        private readonly ICurrentScenario _currentScenario;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly HashSet<ITraceable> _refreshedAccounts = new HashSet<ITraceable>();
        

        public TraceableRefreshService(ICurrentScenario currentScenario, IScheduleRepository scheduleRepository)
        {
            _currentScenario = currentScenario;
            _scheduleRepository = scheduleRepository;
        }

        public bool NeedsRefresh(ITraceable traceable)
        {
            return !_refreshedAccounts.Contains(traceable);
        }

        public void RefreshIfNeeded(IAccount account)
        {
            if (account != null && NeedsRefresh(account)) RefreshAndAddToCache(account);
        }

        public void Refresh(IAccount account)
        {
            if (account != null) RefreshAndAddToCache(account);
        }

        private void RefreshAndAddToCache(IAccount account)
        {
            account.CalculateUsed(_scheduleRepository, _currentScenario.Current());
           _refreshedAccounts.Add(account);
        }
    }
}
