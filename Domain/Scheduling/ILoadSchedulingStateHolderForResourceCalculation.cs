using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ILoadSchedulingStateHolderForResourceCalculation
    {
        void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder, bool loadLight = false);
    }
}