using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class PersonDtoFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly ITenantPeopleLoader _tenantPeopleLoader;

		public PersonDtoFactory(IPersonRepository personRepository, IAssembler<IPerson, PersonDto> personAssembler,
			ITenantPeopleLoader tenantPeopleLoader)
		{
			_personRepository = personRepository;
			_personAssembler = personAssembler;
			_tenantPeopleLoader = tenantPeopleLoader;
		}

		public ICollection<PersonDto> GetPersons(bool excludeLoggedOnPerson)
		{
			ICollection<IPerson> memberList = _personRepository.FindAllSortByName().ToList();

			// Remove logged person
			if (excludeLoggedOnPerson)
			{
				memberList.Remove(TeleoptiPrincipal.CurrentPrincipal.GetPerson(_personRepository));
			}

			return fixPersons(memberList);
		}

		public ICollection<PersonDto> GetPersonTeamMembers(PersonDto person, DateTime utcDate)
		{
			IEnumerable<PersonDto> dtos;

			IPerson thePerson = _personAssembler.DtoToDomainEntity(person);
			TimeZoneInfo timeZoneInfo = thePerson.PermissionInformation.DefaultTimeZone();
			DateOnly dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(utcDate, timeZoneInfo).Date);
			IPersonPeriod personPeriodForGivenDate = thePerson.Period(dateOnlyPerson);

			if (personPeriodForGivenDate != null)
			{
				ICollection<IPerson> personCollection = _personRepository.FindPeopleBelongTeam(personPeriodForGivenDate.Team, personPeriodForGivenDate.Period);
				dtos = fixPersons(personCollection);
			}
			else
			{
				dtos = new List<PersonDto>();
			}
			return dtos.OrderBy(d => d.Name).ToList();
		}

		public ICollection<PersonDto> GetPersonsByTeam(TeamDto team, DateTime utcDateTime, IPersonCollection personCollection)
		{
			var persons = new List<IPerson>();
			foreach (IPerson person in personCollection.AllPermittedPersons)
			{
				DateTime localTime = getPersonLocalTime(utcDateTime, person);
				var localDate = new DateOnly(localTime);
				ITeam personsTeam = person.MyTeam(localDate);
				if (personsTeam != null &&
					personsTeam.Id == team.Id.GetValueOrDefault())
				{
					persons.Add(person);
				}
			}

			return fixPersons(persons);
		}

		public PersonDto GetLoggedOnPerson()
		{
			IPerson currentPerson = TeleoptiPrincipal.CurrentPrincipal.GetPerson(_personRepository);
			return fixPersons(new List<IPerson>{currentPerson}).FirstOrDefault();
		}
		private ICollection<PersonDto> fixPersons(ICollection<IPerson> persons)
		{
			var personCollection = _personAssembler.DomainEntitiesToDtos(persons).ToList();
			_tenantPeopleLoader.FillDtosWithLogonInfo(personCollection);

			return personCollection;
		}

		private static DateTime getPersonLocalTime(DateTime utcTime, IPerson person)
		{
			TimeZoneInfo timeZone = person.PermissionInformation.DefaultTimeZone();
			return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
		}
	}
}