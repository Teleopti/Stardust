using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IRangeProjectionService
    {
        IEnumerable<IVisualLayer> CreateProjection(IScheduleRange scheduleRange, DateTimePeriod period);
    }
}