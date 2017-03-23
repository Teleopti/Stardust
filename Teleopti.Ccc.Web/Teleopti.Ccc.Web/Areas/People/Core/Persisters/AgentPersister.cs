using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Interfaces.Domain;

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
				if (agentResult.Feedback.ErrorMessages.Any() || agentData == null) continue;

				var person = persistPerson(agentData);
				var errorMessages = _tenantUserPersister.Persist(new PersonInfoModel
				{
					ApplicationLogonName = agentData.ApplicationUserId.IsNullOrWhiteSpace() ? null : agentData.ApplicationUserId,
					Identity = agentData.WindowsUser.IsNullOrWhiteSpace() ? null : agentData.WindowsUser,
					Password = agentData.Password.IsNullOrWhiteSpace() ? null : agentData.Password,
					PersonId = person.Id.GetValueOrDefault()
				});

				if (errorMessages.Any())
				{
					_personRepository.HardRemove(person);
					agentResult.Feedback.ErrorMessages.AddRange(errorMessages);
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


			agentData.Skills.ForEach(s => personPeriod.AddPersonSkill(new PersonSkill(s, new Percent(1))));
		
			personPeriod.RuleSetBag = agentData.RuleSetBag;
			foreach (var externalLogOn in agentData.ExternalLogons)
			{
				personPeriod.AddExternalLogOn(externalLogOn);
			}
			person.AddPersonPeriod(personPeriod);
		}

		private IPerson persistPerson(AgentDataModel agentData)
		{
			var person = createPersonFromModel(agentData);
			_personRepository.Add(person);
			return person;
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