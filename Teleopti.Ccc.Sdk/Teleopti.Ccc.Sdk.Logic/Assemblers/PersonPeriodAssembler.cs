using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonPeriodAssembler : Assembler<IPersonPeriod, PersonPeriodDetailDto>
    {
    	private IAssembler<IExternalLogOn, ExternalLogOnDto> _externalLogOnAssembler;

		public PersonPeriodAssembler(IAssembler<IExternalLogOn, ExternalLogOnDto> externalLogOnAssembler)
		{
			_externalLogOnAssembler = externalLogOnAssembler;
		}

    	public override PersonPeriodDetailDto DomainEntityToDto(IPersonPeriod entity)
		{
			if(entity == null)
				throw new ArgumentNullException("entity");
    		var team = entity.Team;
			var personPeriodDto = new PersonPeriodDetailDto();
			
			personPeriodDto.PersonId = entity.Parent.Id.GetValueOrDefault(Guid.Empty);
			personPeriodDto.StartDate = new DateOnlyDto { DateTime = entity.StartDate.Date };
			personPeriodDto.Team = new TeamDto { Id = team.Id, Description = team.Description.Name, SiteAndTeam = team.SiteAndTeam };
			personPeriodDto.Note = entity.Note;
			personPeriodDto.ContractId = entity.PersonContract.Contract.Id.GetValueOrDefault(Guid.Empty);
			personPeriodDto.PartTimePercentageId = entity.PersonContract.PartTimePercentage.Id.GetValueOrDefault(Guid.Empty);
			personPeriodDto.ContractScheduleId = entity.PersonContract.ContractSchedule.Id.GetValueOrDefault(Guid.Empty);

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
