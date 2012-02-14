using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IScheduleDayRestrictor
    {
        IList<IScheduleDay> RemoveScheduleDayEndingTooLate(IList<IScheduleDay> scheduleParts, DateTime givenDate);
    }
}