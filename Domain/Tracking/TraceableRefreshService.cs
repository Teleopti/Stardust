using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class TraceableRefreshService : ITraceableRefreshService
    {
        private readonly HashSet<ITraceable> _refreshedAccounts = new HashSet<ITraceable>();
        private readonly IScenario _scenario;
        private readonly IRepositoryFactory _repositoryFactory;

        public TraceableRefreshService(IScenario scenario, IRepositoryFactory repositoryFactory) {
            _scenario = scenario;
            _repositoryFactory = repositoryFactory;
        }

        public bool NeedsRefresh(ITraceable traceable)
        {
            return !_refreshedAccounts.Contains(traceable);
        }

        public void RefreshIfNeeded(IAccount account, IUnitOfWork unitOfWork)
        {
            if (account != null && NeedsRefresh(account)) RefreshAndAddToCache(account, unitOfWork);
        }

        public void Refresh(IAccount account, IUnitOfWork unitOfWork)
        {
            if (account != null) RefreshAndAddToCache(account, unitOfWork);
        }

        private void RefreshAndAddToCache(IAccount account, IUnitOfWork unitOfWork)
        {
            account.CalculateUsed(_repositoryFactory.CreateScheduleRepository(unitOfWork), _scenario);
            
            _refreshedAccounts.Add(account);
        }
    }
}
