using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ILoadSchedulingStateHolderForResourceCalculation
    {
        void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons);
	    void LoadForRequest(IScenario scenario, DateTimePeriod period, IList<IPerson> requestedPersons);
    }
}