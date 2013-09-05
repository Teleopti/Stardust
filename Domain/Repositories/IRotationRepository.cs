using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRotationRepository : ILoadAggregateById<IRotation>, IRepository<IRotation>
    {
        IList<IRotation> LoadAllRotationsWithDaysAndRestrictions();
        IList<IRotation> LoadAllRotationsWithDays();
    }
}