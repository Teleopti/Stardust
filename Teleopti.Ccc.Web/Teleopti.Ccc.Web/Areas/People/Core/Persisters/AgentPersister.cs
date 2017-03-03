using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public interface IAgentPersister
	{
		void Persist(IEnumerable<AgentExtractionResult> data);
	}

	public class AgentPersister : IAgentPersister
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly ITenantUserPersister _tenantUserPersister;

		public AgentPersister(ILoggedOnUser loggedOnUser, IPersonRepository personRepository,
			ITenantUserPersister tenantUserPersister)
		{
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_tenantUserPersister = tenantUserPersister;
		}

		public void Persist(IEnumerable<AgentExtractionResult> data)
		{
			foreach (var agentResult in data)
			{
				var agentData = agentResult.Agent;
				if (agentData == null) continue;

				var person = persistPerson(agentData);
				var errorMessages =	_tenantUserPersister.Persist(agentData, person.Id.GetValueOrDefault());

				if (errorMessages.Any())
				{
					removePerson(person);
					agentResult.ErrorMessages.AddRange(errorMessages);
					continue;
				}

				addPersonData(person, agentData);				
			}
		}

		private void addPersonData(IPerson person, AgentDataModel agentData)
		{
			addPersonPeriod(person, agentData);
			addSchedulePeriod(person, agentData);
		}

		private void addSchedulePeriod(IPerson person, AgentDataModel agentData)
		{
			var schedulePeriod = new SchedulePeriod(agentData.StartDate, agentData.SchedulePeriodType,
				agentData.SchedulePeriodLength);
			person.AddSchedulePeriod(schedulePeriod);
		}

		private void addPersonPeriod(IPerson person, AgentDataModel agentData)
		{
			var personContract = new PersonContract(agentData.Contract,agentData.PartTimePercentage,agentData.ContractSchedule);
			var personPeriod = new PersonPeriod(agentData.StartDate,personContract,agentData.Team);
			person.AddPersonPeriod(personPeriod);
		}

		private IPerson persistPerson(AgentDataModel agentData)
		{
			var person = createPersonFromModel(agentData);
			_personRepository.Add(person);
			return person;
		}

		private void removePerson(IPerson person)
		{
			_personRepository.Remove(person);
		}

		private IPerson createPersonFromModel(AgentDataModel agentData)
		{
			var person = new Person();
			person.SetName(new Name(agentData.Firstname ?? " ", agentData.Lastname ?? " "));
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			agentData.Roles.ForEach(role => person.PermissionInformation.AddApplicationRole(role));
			return person;
		}
	}

	
}