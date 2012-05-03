using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public class PersonRequestPersister : IPersonRequestPersister
    {
        private readonly IClearReferredShiftTradeRequests _clearReferredShiftTradeRequests;

        public PersonRequestPersister(IClearReferredShiftTradeRequests clearReferredShiftTradeRequests)
        {
            _clearReferredShiftTradeRequests = clearReferredShiftTradeRequests;
        }

        public void MarkForPersist(IPersonRequestRepository personRequestRepository, IEnumerable<IPersonRequest> personRequests)
        {
            if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
                return;

            foreach (var workingPersonRequest in personRequests)
            {
                if (workingPersonRequest.Changed)
                {
                    personRequestRepository.Add(workingPersonRequest);
                }
                workingPersonRequest.Persisted();
            }
            _clearReferredShiftTradeRequests.ClearReferredShiftTradeRequests();
        }
    }
}