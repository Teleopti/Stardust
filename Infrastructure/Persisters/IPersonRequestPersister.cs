using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters 
{
    public interface IPersonRequestPersister
    {
        void MarkForPersist(IPersonRequestRepository personRequestRepository, IEnumerable<IPersonRequest> personRequests);
    }
}