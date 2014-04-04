using System;
using System.Collections.Generic;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{

    public class SkillDataLoader
    {
        private DateTime _dateOnly;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly ServiceApplication _service;

        public SkillDataLoader(DateTime dateOnly, TimeZoneInfo timeZoneInfo, ServiceApplication service)
        {
            _dateOnly = dateOnly;
            _timeZoneInfo = timeZoneInfo;
            _service = service;
        }

        public IList<SkillDay> Load()
        {
            IList<SkillDay> retList = new List<SkillDay>();
            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = _dateOnly.Date;
            dateOnlyDto.DateTimeSpecified = true;
            IList<SkillDayDto> temp =
                new List<SkillDayDto>(
                    _service.ForecastingService.GetSkillDataByQuery(new GetSkillDaysByPeriodQueryDto
                    {
                        Period = new DateOnlyPeriodDto {StartDate = dateOnlyDto, EndDate = dateOnlyDto},
                        TimeZoneId = _timeZoneInfo.Id
                    }));
            foreach (SkillDayDto dto in temp)
            {
                retList.Add(new SkillDay(dto));
            }

            return retList;
        }
    }
}
