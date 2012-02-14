using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public abstract class ScheduleLoadOptions
    {
        public abstract IEnumerable<SchedulePartDto> LoadScheduleParts(ITeleoptiSchedulingService schedulingService);
    }
}