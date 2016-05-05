using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleCommandHandlingProvider : ITeamScheduleCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;

		public TeamScheduleCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, IPersonRepository personRepository, IPermissionProvider permissionProvider)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
		}

		public List<FailActionResult> AddActivity(AddActivityFormData input)
		{
			var permissions = new Dictionary<string,string>
			{
				{  DefinedRaptorApplicationFunctionPaths.AddActivity,  Resources.NoPermissionAddAgentActivity}
			};

			var result = new List<FailActionResult>();
			foreach (var personId in input.PersonIds)
			{
				var actionResult = new FailActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.Messages = new List<string>();

				if(checkPermission(permissions,input.Date, person,actionResult.Messages))
				{
					var command = new AddActivityCommand
					{
						PersonId = personId,
						ActivityId = input.ActivityId,
						Date = input.Date,
						StartTime = input.StartTime,
						EndTime = input.EndTime,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value}
					};
					_commandDispatcher.Execute(command);
				}

				if( actionResult.Messages.Any())
					result.Add(actionResult);				
			}

			return result;
		}

		public IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds)
		{
			var agents = _personRepository.FindPeople(agentIds);
			return agents.Where(agent => agentScheduleIsWriteProtected(date, agent)).Select(x => x.Id.GetValueOrDefault()).ToList();
		}
		
		public List<FailActionResult> RemoveActivity(RemoveActivityFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.RemoveActivity, Resources.NoPermissionRemoveAgentActivity}
			};

			var result = new List<FailActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var actionResult = new FailActionResult();
				var person = _personRepository.Get(personActivity.PersonId);
				actionResult.PersonId = personActivity.PersonId;
				actionResult.Messages = new List<string>();
				if (checkPermission(permissions, input.Date, person, actionResult.Messages))				
				{
					foreach(var shiftLayerId in personActivity.ShiftLayerIds)
					{
						var command = new RemoveActivityCommand
						{
							PersonId = personActivity.PersonId,
							ShiftLayerId = shiftLayerId,
							Date = input.Date,
							TrackedCommandInfo =
								input.TrackedCommandInfo != null
									? input.TrackedCommandInfo
									: new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
						};

						_commandDispatcher.Execute(command);
						if(command.ErrorMessages != null && command.ErrorMessages.Any())
						{
							actionResult.Messages.AddRange(command.ErrorMessages);
						}

					}					
				}
				if (actionResult.Messages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		public List<FailActionResult> MoveActivity(MoveActivityFormData input)
		{
			var permission = new Dictionary<string, string>
			{
				{DefinedRaptorApplicationFunctionPaths.MoveActivity, Resources.NoPermissionMoveAgentActivity}
			};
			var result = new List<FailActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);
				var personError = new FailActionResult {PersonId = person.Id.GetValueOrDefault(), Messages = new List<string>()};
				if (!checkPermission(permission, input.Date, person, personError.Messages))
				{
					result.Add(personError);
					continue;
				}
			}

			return result;
		}

		private bool agentScheduleIsWriteProtected(DateOnly date,IPerson agent)
		{
			return !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				&& agent.PersonWriteProtection.IsWriteProtected(date);
		}

		private bool checkPermission(Dictionary<string,string> permissions,DateOnly date,IPerson agent,IList<string> messages)
		{
			var newMessages = new List<string>();

			if(agentScheduleIsWriteProtected(date,agent))
			{
				newMessages.Add(Resources.WriteProtectSchedule);
			}

			newMessages.AddRange(
				from permission in permissions.Keys
				where !_permissionProvider.HasPersonPermission(permission,date,agent)
				select permissions[permission]);

			messages.AddRange(newMessages);
			return !newMessages.Any();
		}
	}

	public interface ITeamScheduleCommandHandlingProvider
	{
		List<FailActionResult> AddActivity(AddActivityFormData formData);		
		IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds);
		List<FailActionResult> RemoveActivity(RemoveActivityFormData input);
		List<FailActionResult> MoveActivity(MoveActivityFormData input);
	}
}