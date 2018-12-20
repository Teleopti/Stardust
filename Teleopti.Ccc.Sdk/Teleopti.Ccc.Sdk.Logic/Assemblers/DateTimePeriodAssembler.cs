using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;


namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class DateTimePeriodAssembler : Assembler<DateTimePeriod,DateTimePeriodDto>, IDateTimePeriodAssembler
	{
        public DateTimePeriodAssembler()
        {
            TimeZone = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone;
        }

        public TimeZoneInfo TimeZone { get; set; }

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
            if (dto.UtcStartTime == DateTime.MinValue)
            {
                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dto.LocalStartDateTime,
                                                                              dto.LocalEndDateTime, TimeZone);
            }
            if (dto.UtcStartTime != DateTime.MinValue)
            {
                return new DateTimePeriod(dto.UtcStartTime,dto.UtcEndTime);
            }
            return new DateTimePeriod();
        }
    }
}