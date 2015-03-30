using System;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonAccountAssembler : ScheduleDataAssembler<IAccount, PersonAccountDto>
    {
        public override PersonAccountDto DomainEntityToDto(IAccount entity)
        {
            PersonAccountDto personAccountDto = new PersonAccountDto();
            personAccountDto.IsInMinutes = (entity.GetType() == typeof (AccountTime));        
            personAccountDto.Accrued = entity.Accrued.Ticks;
            personAccountDto.BalanceIn = entity.BalanceIn.Ticks;
            personAccountDto.Extra = entity.Extra.Ticks;
            personAccountDto.LatestCalculatedBalance = entity.LatestCalculatedBalance.Ticks;
            personAccountDto.TrackingDescription = entity.Owner.Absence.Name;
			personAccountDto.Period = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.Period().StartDate.Date },
				EndDate = new DateOnlyDto { DateTime = entity.Period().EndDate.Date }
			};
            personAccountDto.Remaining = entity.Remaining.Ticks;
            personAccountDto.BalanceOut = entity.BalanceOut.Ticks;
            return personAccountDto;
        }

        protected override IAccount DtoToDomainEntityAfterValidation(PersonAccountDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
