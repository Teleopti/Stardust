using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonPeriodAssembler : Assembler<IPersonPeriod, PersonPeriodDetailDto>
    {
    	private readonly IAssembler<IExternalLogOn, ExternalLogOnDto> _externalLogOnAssembler;

		public PersonPeriodAssembler(IAssembler<IExternalLogOn, ExternalLogOnDto> externalLogOnAssembler)
		{
			_externalLogOnAssembler = externalLogOnAssembler;
		}

    	public override PersonPeriodDetailDto DomainEntityToDto(IPersonPeriod entity)
		{
			if(entity == null)
				throw new ArgumentNullException(nameof(entity));
    		var team = entity.Team;
			var personPeriodDto = new PersonPeriodDetailDto();
			
			personPeriodDto.PersonId = entity.Parent.Id.GetValueOrDefault();
			personPeriodDto.StartDate = new DateOnlyDto { DateTime = entity.StartDate.Date };
			personPeriodDto.Team = new TeamDto { Id = team.Id, Description = team.Description.Name, SiteAndTeam = team.SiteAndTeam };
			personPeriodDto.Note = entity.Note;
			personPeriodDto.ContractId = entity.PersonContract.Contract.Id.GetValueOrDefault();
			personPeriodDto.PartTimePercentageId = entity.PersonContract.PartTimePercentage.Id.GetValueOrDefault();
			personPeriodDto.ContractScheduleId = entity.PersonContract.ContractSchedule.Id.GetValueOrDefault();

			foreach (var externalLogOn in entity.ExternalLogOnCollection)
			{
				personPeriodDto.ExternalLogOn.Add(_externalLogOnAssembler.DomainEntityToDto(externalLogOn));
			}
			
			
			return personPeriodDto;
		}

		public override IPersonPeriod DtoToDomainEntity(PersonPeriodDetailDto dto)
		{
			throw new NotImplementedException();
		}
	}
}
