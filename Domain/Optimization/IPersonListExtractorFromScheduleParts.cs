using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IPersonListExtractorFromScheduleParts
    {
		IList<IPerson> ExtractPersons(IEnumerable<IScheduleDay> scheduleDays);
    }
}