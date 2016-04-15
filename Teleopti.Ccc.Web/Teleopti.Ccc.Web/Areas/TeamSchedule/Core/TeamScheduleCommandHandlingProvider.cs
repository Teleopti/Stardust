using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly IPermissionProvider _permissionProvider;

		public TeamScheduleCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, IPrincipalAuthorization principalAuthorization, IPersonRepository personRepository, IPermissionProvider permissionProvider)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;		
			_principalAuthorization = principalAuthorization;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
		}

		public List<FailActionResult> AddActivity(AddActivityFormData formData)
		{
			var result = new List<FailActionResult>();
			foreach (var personId in formData.PersonIds)
			{
				if (_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddActivity,
					formData.BelongsToDate, _personRepository.Load(personId)))
				{
					var command = new AddActivityCommand
					{
						PersonId = personId,
						ActivityId = formData.ActivityId,
						Date = formData.BelongsToDate,
						StartTime = formData.StartTime,
						EndTime = formData.EndTime,
						TrackedCommandInfo =
							formData.TrackedCommandInfo != null
								? formData.TrackedCommandInfo
								: new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value}
					};
					_commandDispatcher.Execute(command);
				}
				else
				{
					result.Add(new FailActionResult()
					{
						PersonId = personId,
						Message = new List<string> { Resources.NoPermissionAddAgentActivity}
					});
				}
			}

			return result;
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

		public List<FailActionResult> RemoveActivity(RemoveActivityFormData input)
		{

			var result = new List<FailActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{

				if (_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.RemoveActivity, input.Date,
					_personRepository.Load(personActivity.PersonId)))
				{
					foreach (var shiftLayerId in personActivity.ShiftLayerIds)
					{
						var command = new RemoveActivityCommand
						{
							PersonId = personActivity.PersonId,
							ShiftLayerId = shiftLayerId,
							Date = input.Date,
							TrackedCommandInfo =
								input.TrackedCommandInfo != null
									? input.TrackedCommandInfo
									: new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value}
						};

						_commandDispatcher.Execute(command);
						if (command.ErrorMessages != null && command.ErrorMessages.Any())
							result.Add(new FailActionResult
							{
								PersonId = personActivity.PersonId,
								Message = command.ErrorMessages
							});
					}
				}
				else
				{
					result.Add(new FailActionResult
					{
						PersonId = personActivity.PersonId,
						Message = new List<string> { Resources.NoPermissionRemoveAgentActivity}
					});
				}
			}

			return result;
		}
	}

	public interface ITeamScheduleCommandHandlingProvider
	{
		List<FailActionResult> AddActivity(AddActivityFormData formData);		
		IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds);
		List<FailActionResult> RemoveActivity(RemoveActivityFormData input);
	}
}