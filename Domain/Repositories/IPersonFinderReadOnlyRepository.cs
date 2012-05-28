using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonFinderReadOnlyRepository
    {
        void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
        void UpdateFindPerson(string ids );
        void UpdateFindPersonData(string ids);
    }
}
