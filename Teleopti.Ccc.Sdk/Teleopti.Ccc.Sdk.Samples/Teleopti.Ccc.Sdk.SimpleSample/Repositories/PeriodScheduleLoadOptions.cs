using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PeriodScheduleLoadOptions : ScheduleLoadOptions
    {
        private readonly DateOnlyDto _queryDate;
    	private readonly Guid _groupPageGroupId;
        private readonly TimeZoneInfo _timeZoneInfo;

        public PeriodScheduleLoadOptions(DateOnlyDto queryDate,Guid groupPageGroupId,TimeZoneInfo timeZoneInfo)
        {
            _queryDate = queryDate;
        	_groupPageGroupId = groupPageGroupId;
            _timeZoneInfo = timeZoneInfo;
        }

        public override IEnumerable<SchedulePartDto> LoadScheduleParts(ITeleoptiSchedulingService schedulingService)
        {
        	return
        		schedulingService.GetSchedulesByQuery(new GetSchedulesByGroupPageGroupQueryDto
        		                                      	{
        		                                      		QueryDate = _queryDate,
        		                                      		GroupPageGroupId = _groupPageGroupId,
        		                                      		TimeZoneId = _timeZoneInfo.Id
        		                                      	});
        }
    }
}