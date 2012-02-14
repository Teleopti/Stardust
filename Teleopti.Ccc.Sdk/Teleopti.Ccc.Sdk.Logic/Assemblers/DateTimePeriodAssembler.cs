using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class DateTimePeriodAssembler : Assembler<DateTimePeriod,DateTimePeriodDto>
    {
        public DateTimePeriodAssembler()
        {
            TimeZone = StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone;
        }

        public ICccTimeZoneInfo TimeZone { get; set; }

        public override DateTimePeriodDto DomainEntityToDto(DateTimePeriod entity)
        {
            return new DateTimePeriodDto
                       {
                           UtcStartTime = entity.StartDateTime,
                           UtcEndTime = entity.EndDateTime,
                           LocalStartDateTime = entity.StartDateTimeLocal(TimeZone),
                           LocalEndDateTime = entity.EndDateTimeLocal(TimeZone)
                       };
        }

        public override DateTimePeriod DtoToDomainEntity(DateTimePeriodDto dto)
        {
            DateTimePeriod period = new DateTimePeriod();
            if (dto.UtcStartTime == DateTime.MinValue)
            {
                period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dto.LocalStartDateTime,
                                                                              dto.LocalEndDateTime, TimeZone);
            }
            if (dto.UtcStartTime != DateTime.MinValue)
            {
                period = new DateTimePeriod(dto.UtcStartTime,dto.UtcEndTime);
            }
            return period;
        }
    }
}