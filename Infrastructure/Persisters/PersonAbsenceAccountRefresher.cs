using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public interface IPersonAbsenceAccountRefresher
    {
        void Refresh(IUnitOfWork unitOfWork, IPersonAbsenceAccount personAbsenceAccount);
    }

    public class PersonAbsenceAccountRefresher : IPersonAbsenceAccountRefresher
    {
        private readonly Func<ITraceableRefreshService> _traceableRefreshService;

        public PersonAbsenceAccountRefresher(IRepositoryFactory repositoryFactory, IScenario scenario)
            : this(() => new TraceableRefreshService(scenario, repositoryFactory))
        {
        }

        public PersonAbsenceAccountRefresher(Func<ITraceableRefreshService> traceableRefreshService)
        {
            _traceableRefreshService = traceableRefreshService;
        }

        public void Refresh(IUnitOfWork unitOfWork, IPersonAbsenceAccount personAbsenceAccount)
        {
            var traceableRefreshService = _traceableRefreshService.Invoke();
            personAbsenceAccount.AccountCollection().ForEach(a => traceableRefreshService.Refresh(a, unitOfWork));
        }
    }
}