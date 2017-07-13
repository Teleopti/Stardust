using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class TraceableRefreshService : ITraceableRefreshService
    {
        private readonly ICurrentScenario _currentScenario;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly HashSet<ITraceable> _refreshedAccounts = new HashSet<ITraceable>();
        

        public TraceableRefreshService(ICurrentScenario currentScenario, IScheduleStorage scheduleStorage)
        {
            _currentScenario = currentScenario;
            _scheduleStorage = scheduleStorage;
        }

        public bool NeedsRefresh(ITraceable traceable)
        {
            return !_refreshedAccounts.Contains(traceable);
        }

        public void RefreshIfNeeded(IAccount account)
        {
            if (account != null && NeedsRefresh(account)) refreshAndAddToCache(account);
        }

        public void Refresh(IAccount account)
        {
            if (account != null) refreshAndAddToCache(account);
        }

        private void refreshAndAddToCache(IAccount account)
        {
            account.CalculateUsed(_scheduleStorage, _currentScenario.Current());
           _refreshedAccounts.Add(account);
        }
    }
}
