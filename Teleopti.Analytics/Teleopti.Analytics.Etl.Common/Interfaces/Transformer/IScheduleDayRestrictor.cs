using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IScheduleDayRestrictor
    {
        IList<IScheduleDay> RemoveScheduleDayEndingTooLate(IList<IScheduleDay> scheduleParts, DateTime givenDate);
    }
}