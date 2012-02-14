using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak
    {
        bool CanHaveShortBreak(IEnumerable<IPerson> persons, DateOnlyPeriod period);
    }
}