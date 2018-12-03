using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class PersonsFromLoadOptionFactory
	{
	    private readonly IPersonRepository _personRepository;
	    private readonly ITeamRepository _teamRepository;
	    private readonly IAssembler<IPerson, PersonDto> _personAssembler;

	    public PersonsFromLoadOptionFactory(IPersonRepository personRepository, ITeamRepository teamRepository, IAssembler<IPerson,PersonDto> personAssembler)
	    {
	        _personRepository = personRepository;
	        _teamRepository = teamRepository;
	        _personAssembler = personAssembler;
	    }

		internal ICollection<PersonDto> GetPersonFromLoadOption(PublicNoteLoadOptionDto publicNoteLoadOptionDto, ICollection<TeamDto> teamDtos, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			CheckPublicNoteLoadOption(publicNoteLoadOptionDto);
			ICollection<PersonDto> personDtos = new Collection<PersonDto>();

			if (publicNoteLoadOptionDto.LoadSite != null)
			{
				personDtos = GetPersonsOnSite(teamDtos, startDate, endDate);
			}
			else if (publicNoteLoadOptionDto.LoadTeam != null)
			{
				personDtos = GetPersonsOnTeam(publicNoteLoadOptionDto.LoadTeam, startDate, endDate);
			}
			else if (publicNoteLoadOptionDto.LoadPerson != null)
			{
				personDtos = GetPersonsOnPerson(publicNoteLoadOptionDto.LoadPerson);
			}

			return personDtos;
		}

		private static void CheckPublicNoteLoadOption(PublicNoteLoadOptionDto publicNoteLoadOptionDto)
		{
			int countParametersSet = 0;
			if (publicNoteLoadOptionDto.LoadSite != null) countParametersSet++;
			if (publicNoteLoadOptionDto.LoadTeam != null) countParametersSet++;
			if (publicNoteLoadOptionDto.LoadPerson != null) countParametersSet++;

			if (countParametersSet != 1)
			{
				throw new FaultException("publicNoteLoadOptionDto must have exact one option specified.");
			}
		}

		private ICollection<PersonDto> GetPersonsOnSite(IEnumerable<TeamDto> teamDtos, DateOnlyDto startDate, DateOnlyDto endDate)
		{
		    using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
		    {
		        var persons = new List<IPerson>();
		        var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
		        var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));

		        foreach (var teamDto in teamDtos)
		        {
		            if (teamDto.Id == null) continue;
		            var team = _teamRepository.Load(teamDto.Id.Value);
		            persons.AddRange(_personRepository.FindPeopleBelongTeam(team, period));
		        }

		        return _personAssembler.DomainEntitiesToDtos(persons).ToList();
		    }
		}

	    private ICollection<PersonDto> GetPersonsOnTeam(TeamDto teamDto, DateOnlyDto startDate, DateOnlyDto endDate)
	    {
	        if (teamDto == null) throw new ArgumentNullException(nameof(teamDto));

	        using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
	        {
	            ICollection<IPerson> personList = new List<IPerson>();
	            if (teamDto.Id != null)
	            {
	                var team = _teamRepository.Load(teamDto.Id.Value);
	                var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
	                var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));
	                personList = _personRepository.FindPeopleBelongTeam(team, period);
	            }

	            return _personAssembler.DomainEntitiesToDtos(personList).ToList();
	        }
	    }

	    private static ICollection<PersonDto> GetPersonsOnPerson(PersonDto personDto)
		{
			return new Collection<PersonDto> { personDto };
		}
	}
}
