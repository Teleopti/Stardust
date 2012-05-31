using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonFinderReadOnlyRepository
    {
        void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
        void UpdateFindPerson(Guid[] ids );
        void UpdateFindPersonData(Guid[] ids);
    }
}
