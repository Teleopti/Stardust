using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleCommandHandlingProvider : ITeamScheduleCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly IPrincipalAuthorization _principalAuthorization;

		public TeamScheduleCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, IPrincipalAuthorization principalAuthorization, IPersonRepository personRepository)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;		
			_principalAuthorization = principalAuthorization;
			_personRepository = personRepository;
		}

		public void AddActivity(AddActivityFormData formData)
		{
			foreach (var person in formData.PersonIds)
			{
				var command = new AddActivityCommand
				{
					PersonId = person,
					ActivityId = formData.ActivityId,
					Date = formData.BelongsToDate,
					StartTime = formData.StartTime,
					EndTime = formData.EndTime,
					TrackedCommandInfo = formData.TrackedCommandInfo != null ? formData.TrackedCommandInfo : new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
				};
				_commandDispatcher.Execute(command);
			}
		}

		public IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds)
		{
			if (_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
			{
				return new List<Guid>();
			}

			var agents = _personRepository.FindPeople(agentIds);

			return agents.Where(agent => agent.PersonWriteProtection.IsWriteProtected(date)).Select(agent => agent.Id.GetValueOrDefault()); 		
		}
	}

	public interface ITeamScheduleCommandHandlingProvider
	{
		void AddActivity(AddActivityFormData formData);		
		IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds);
	}
}