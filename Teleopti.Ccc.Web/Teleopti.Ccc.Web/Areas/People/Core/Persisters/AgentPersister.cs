using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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

		public AgentPersister(ILoggedOnUser loggedOnUser, IPersonRepository personRepository, ITenantUserPersister tenantUserPersister)
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

				// [ToDo] Create new person period

				// [ToDo] Create new schedule period

			}
		}


		[UnitOfWork]
		private IPerson persistPerson(AgentDataModel agentData)
		{
			var person = createPersonFromModel(agentData);
			_personRepository.Add(person);
			return person;
		}

		[UnitOfWork]
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