using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonAccountAssembler : ScheduleDataAssembler<IAccount, PersonAccountDto>
    {
        public override PersonAccountDto DomainEntityToDto(IAccount entity)
        {
	        var period = entity.Period();
	        var personAccountDto =
		        new PersonAccountDto
		        {
			        IsInMinutes = entity.GetType() == typeof(AccountTime),
			        Accrued = entity.Accrued.Ticks,
			        BalanceIn = entity.BalanceIn.Ticks,
			        Extra = entity.Extra.Ticks,
			        LatestCalculatedBalance = entity.LatestCalculatedBalance.Ticks,
			        TrackingDescription = entity.Owner.Absence.Name,
			        Period = new DateOnlyPeriodDto
			        {
				        StartDate = new DateOnlyDto {DateTime = period.StartDate.Date},
				        EndDate = new DateOnlyDto {DateTime = period.EndDate.Date}
			        },
			        Remaining = entity.Remaining.Ticks,
			        BalanceOut = entity.BalanceOut.Ticks
		        };

	        return personAccountDto;
        }

        protected override IAccount DtoToDomainEntityAfterValidation(PersonAccountDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
