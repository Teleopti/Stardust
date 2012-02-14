using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonFinderReadOnlyRepository
    {
        void Find(IPersonFinderSearchCriteria personFinderSearchCriteria);
    }
}
