using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PeriodScheduleLoadOptions : ScheduleLoadOptions
    {
        private readonly DateOnlyDto _startDate;
        private readonly DateOnlyDto _endDate;
        private readonly TimeZoneInfo _timeZoneInfo;

        public PeriodScheduleLoadOptions(DateOnlyDto startDate,DateOnlyDto endDate,TimeZoneInfo timeZoneInfo)
        {
            _startDate = startDate;
            _endDate = endDate;
            _timeZoneInfo = timeZoneInfo;
        }

        public override IEnumerable<SchedulePartDto> LoadScheduleParts(ITeleoptiSchedulingService schedulingService)
        {
            return schedulingService.GetSchedules(new ScheduleLoadOptionDto {LoadAll = true}, _startDate, _endDate,
                                                  _timeZoneInfo.Id);
        }
    }
}