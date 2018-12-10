using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ILoadSchedulingStateHolderForResourceCalculation
    {
        void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder, Func<DateOnlyPeriod, ICollection<IPerson>> optionalLoadOrganizationFunc = null, bool loadLight = false);
    }
}