using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public interface IAgentPersister
	{
		dynamic Persist(IEnumerable<AgentExtractionResult> data, TimeZoneInfo timezone);
		void Rollback(dynamic persisted);
	}

	public class AgentPersister : IAgentPersister
	{
		private readonly IPersonRepository _personRepository;
		private readonly ITenantUserPersister _tenantUserPersister;
		public AgentPersister(IPersonRepository personRepository,
			ITenantUserPersister tenantUserPersister)
		{
			_personRepository = personRepository;
			_tenantUserPersister = tenantUserPersister;
		}

		public dynamic Persist(IEnumerable<AgentExtractionResult> data, TimeZoneInfo timezone)
		{
			var persistedAgentIds = new List<Guid>();
			var persistedTenantUserIds = new List<Guid>();
			foreach (var agentResult in data)
			{
				var agentData = agentResult.Agent;
				if (agentResult.Feedback.ErrorMessages.Any() || agentData == null) continue;

				var person = persistPerson(agentData, timezone);
				var personInfo = new PersonInfoModel
				{
					ApplicationLogonName = agentData.ApplicationUserId.IsNullOrEmpty() ? null : agentData.ApplicationUserId,
					Identity = agentData.WindowsUser.IsNullOrEmpty() ? null : agentData.WindowsUser,
					Password = agentData.Password.IsNullOrEmpty() ? null : agentData.Password,
					PersonId = person.Id.GetValueOrDefault()
				};
				var tenantUserResult = _tenantUserPersister.Persist(personInfo);
				var errorMessages = tenantUserResult.ErrorMessages;

				if (errorMessages.Count > 0)
				{
					_personRepository.HardRemove(person);
					agentResult.Feedback.ErrorMessages.AddRange(errorMessages);
					continue;
				}

				addPersonData(person, agentData);

				persistedAgentIds.Add(person.Id.GetValueOrDefault());
				persistedTenantUserIds.Add(tenantUserResult.TenantUserId);
			}

			return new
			{
				AgentIds = persistedAgentIds,
				TenantUserIds = persistedTenantUserIds
			};
		}

		public void Rollback(dynamic persisted)
		{
			var agents = _personRepository.FindPeople(persisted.AgentIds);
			foreach (var person in agents)
			{
				person.RemoveAllPersonPeriods();
				person.RemoveAllSchedulePeriods();
				_personRepository.HardRemove(person);
				
			}
			_tenantUserPersister.RollbackAllPersistedTenantUsers(persisted.TenantUserIds);
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
			var personContract = new PersonContract(agentData.Contract, agentData.PartTimePercentage, agentData.ContractSchedule);
			var personPeriod = new PersonPeriod(agentData.StartDate, personContract, agentData.Team);


			agentData.Skills.ForEach(s => personPeriod.AddPersonSkill(new PersonSkill(s, new Percent(1))));

			personPeriod.RuleSetBag = agentData.RuleSetBag;
			foreach (var externalLogOn in agentData.ExternalLogons)
			{
				personPeriod.AddExternalLogOn(externalLogOn);
			}
			person.AddPersonPeriod(personPeriod);
		}

		private IPerson persistPerson(AgentDataModel agentData, TimeZoneInfo timezone)
		{
			var person = createPersonFromModel(agentData, timezone);
			_personRepository.Add(person);
			return person;
		}

		private IPerson createPersonFromModel(AgentDataModel agentData, TimeZoneInfo timezone)
		{
			var person = new Person();
			person.SetName(new Name(agentData.Firstname ?? " ", agentData.Lastname ?? " "));

			person.PermissionInformation.SetDefaultTimeZone(timezone);
			agentData.Roles.ForEach(role => person.PermissionInformation.AddApplicationRole(role));
			return person;
		}
	}


}