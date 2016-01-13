using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IPersonListExtractorFromScheduleParts
    {
		IList<IPerson> ExtractPersons(IEnumerable<IScheduleDay> scheduleDays);
    }
}