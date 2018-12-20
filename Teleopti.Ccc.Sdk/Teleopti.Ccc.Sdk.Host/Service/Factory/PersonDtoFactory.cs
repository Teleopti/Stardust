using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class PersonDtoFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly PersonCredentialsAppender _credentialsAppender;

		public PersonDtoFactory(IPersonRepository personRepository, IAssembler<IPerson, PersonDto> personAssembler, PersonCredentialsAppender credentialsAppender)
		{
			_personRepository = personRepository;
			_personAssembler = personAssembler;
			_credentialsAppender = credentialsAppender;
		}

		public ICollection<PersonDto> GetPersons(bool excludeLoggedOnPerson)
		{
			ICollection<IPerson> memberList = _personRepository.FindAllSortByName().ToList();

			// Remove logged person
			if (excludeLoggedOnPerson)
			{
				memberList.Remove(_personRepository.Get(TeleoptiPrincipalForLegacy.CurrentPrincipal.PersonId));
			}

			return _credentialsAppender.Convert(memberList.ToArray());
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
				var personCollection = _personRepository.FindPeopleBelongTeam(personPeriodForGivenDate.Team, personPeriodForGivenDate.Period);
				dtos = _credentialsAppender.Convert(personCollection.ToArray());
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

			return _credentialsAppender.Convert(persons.ToArray());
		}

		public PersonDto GetLoggedOnPerson()
		{
			IPerson currentPerson = _personRepository.Get(TeleoptiPrincipalForLegacy.CurrentPrincipal.PersonId);
			return _credentialsAppender.Convert(currentPerson).FirstOrDefault();
		}
		

		private static DateTime getPersonLocalTime(DateTime utcTime, IPerson person)
		{
			TimeZoneInfo timeZone = person.PermissionInformation.DefaultTimeZone();
			return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
		}
	}
}