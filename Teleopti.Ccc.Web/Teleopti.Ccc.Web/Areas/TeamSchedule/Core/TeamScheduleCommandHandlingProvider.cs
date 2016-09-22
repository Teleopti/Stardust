using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
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
		private readonly IMoveShiftLayerCommandHelper _helper;

		public TeamScheduleCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, IPersonRepository personRepository, IPermissionProvider permissionProvider, IMoveShiftLayerCommandHelper helper)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_helper = helper;
		}

		public List<ActionResult> AddActivity(AddActivityFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.AddActivity,  Resources.NoPermissionAddAgentActivity}
			};

			var result = new List<ActionResult>();
			foreach (var personId in input.PersonIds)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();
				actionResult.WarningMessages = new List<string>();

				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new AddActivityCommand
					{
						PersonId = personId,
						ActivityId = input.ActivityId,
						Date = input.Date,
						StartTime = input.StartTime,
						EndTime = input.EndTime,
						MoveConflictLayerAllowed = input.MoveConflictLayerAllowed,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
					};
					_commandDispatcher.Execute(command);
					if (command.ErrorMessages != null && command.ErrorMessages.Any())
					{
						actionResult.ErrorMessages.AddRange(command.ErrorMessages);
					}
					if (command.WarningMessages != null && command.WarningMessages.Any())
					{
						actionResult.WarningMessages.AddRange(command.WarningMessages);
					}
				}

				if (actionResult.ErrorMessages.Any() || actionResult.WarningMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		public List<ActionResult> AddPersonalActivity(AddPersonalActivityFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.AddPersonalActivity,  Resources.NoPermissionAddPersonalActivity}
			};

			var result = new List<ActionResult>();
			foreach (var personId in input.PersonIds)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();

				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new AddPersonalActivityCommand
					{
						PersonId = personId,
						PersonalActivityId = input.ActivityId,
						Date = input.Date,
						StartTime = input.StartTime,
						EndTime = input.EndTime,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
					};
					_commandDispatcher.Execute(command);
					if (command.ErrorMessages != null && command.ErrorMessages.Any())
					{
						actionResult.ErrorMessages.AddRange(command.ErrorMessages);
					}
				}

				if (actionResult.ErrorMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		public IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds)
		{
			var agents = _personRepository.FindPeople(agentIds);
			return agents.Where(agent => agentScheduleIsWriteProtected(date, agent)).Select(x => x.Id.GetValueOrDefault()).ToList();
		}

		public List<ActionResult> RemoveActivity(RemoveActivityFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.RemoveActivity, Resources.NoPermissionRemoveAgentActivity}
			};

			var result = new List<ActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personActivity.PersonId);
				actionResult.PersonId = personActivity.PersonId;
				actionResult.ErrorMessages = new List<string>();
				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
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
									: new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
						};

						_commandDispatcher.Execute(command);
						if (command.ErrorMessages != null && command.ErrorMessages.Any())
						{
							actionResult.ErrorMessages.AddRange(command.ErrorMessages);
						}

					}
				}
				if (actionResult.ErrorMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		public List<ActionResult> BackoutScheduleChange(BackoutScheduleChangeFormData input)
		{
			var permissions = new Dictionary<string, string>();

			var result = new List<ActionResult>();
			foreach(var personId in input.PersonIds)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();
				if(checkPermissionFn(permissions, input.Date,person,actionResult.ErrorMessages))
				{
					var command = new BackoutScheduleChangeCommand
					{
						PersonId = personId,
						Date = input.Date,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value}
					};

					_commandDispatcher.Execute(command);
					if(command.ErrorMessages != null && command.ErrorMessages.Any())
					{
						actionResult.ErrorMessages.AddRange(command.ErrorMessages);
					}					
				}
				if(actionResult.ErrorMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}


		public List<ActionResult> MoveActivity(MoveActivityFormData input)
		{
			var permission = new Dictionary<string, string>
			{
				{DefinedRaptorApplicationFunctionPaths.MoveActivity, Resources.NoPermissionMoveAgentActivity}
			};
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.StartTime, userTimezone);
			var result = new List<ActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);
				var personError = new ActionResult { PersonId = person.Id.GetValueOrDefault(), ErrorMessages = new List<string>() };
				if (!checkPermissionFn(permission, input.Date, person, personError.ErrorMessages))
				{
					result.Add(personError);
					continue;
				}
				var layerToMoveTimeMap = _helper.GetCorrectNewStartForLayersForPerson(person, input.Date, personActivity.ShiftLayerIds,
					newStartTimeInUtc);
				if (personActivity.ShiftLayerIds.Any(x => !layerToMoveTimeMap.ContainsKey(x)))
				{
					personError.ErrorMessages.Add(Resources.NoShiftsFound);
				}

				if (_helper.ValidateLayerMoveToTime(layerToMoveTimeMap, person, input.Date))
				{
					layerToMoveTimeMap
						.ForEach(
							pl =>
							{
								var command = new MoveShiftLayerCommand
								{
									AgentId = personActivity.PersonId,
									NewStartTimeInUtc = pl.Value,
									ScheduleDate = input.Date,
									ShiftLayerId = pl.Key,
									TrackedCommandInfo =
										input.TrackedCommandInfo ??
										new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault()}
								};
								_commandDispatcher.Execute(command);
								if (command.ErrorMessages != null && command.ErrorMessages.Any())
								{
									personError.ErrorMessages.AddRange(command.ErrorMessages);
								}
							});

				}
				else
				{
					personError.ErrorMessages.Add(Resources.ShiftLengthExceed36Hours);
				}

				
				if (personError.ErrorMessages.Any())
				{
					result.Add(personError);
				}
			}

			return result;
		}

		public List<ActionResult> ChangeShiftCategory(ChangeShiftCategoryFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{ DefinedRaptorApplicationFunctionPaths.EditShiftCategory, Resources.NoPermissionToEditShiftCategory }
			};

			var result = new List<ActionResult>();

			foreach (var personId in input.PersonIds)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();

				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new ChangeShiftCategoryCommand
					{
						PersonId = personId,
						Date = input.Date,
						ShiftCategoryId = input.ShiftCategoryId,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo {OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value}
					};

					_commandDispatcher.Execute(command);
					if (command.ErrorMessages != null && command.ErrorMessages.Any())
					{
						actionResult.ErrorMessages.AddRange(command.ErrorMessages);
					}
				}

				if (actionResult.ErrorMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		public IList<ActionResult> MoveNonoverwritableLayers(MoveNonoverwritableLayersFormData input)
		{
			var permissions = new Dictionary<string,string>
			{
				{ DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, Resources.NoPermissionToMoveInvalidOverlappedActivity }
			};

			var result = new List<ActionResult>();

			foreach(var personId in input.PersonIds)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();

				if(checkPermissionFn(permissions,input.Date,person,actionResult.ErrorMessages))
				{
					var command = new FixNotOverwriteLayerCommand
					{
						PersonId = personId,
						Date = input.Date,
						TrackedCommandInfo =
							input.TrackedCommandInfo != null
								? input.TrackedCommandInfo
								: new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
					};

					_commandDispatcher.Execute(command);
					if(command.ErrorMessages != null && command.ErrorMessages.Any())
					{
						actionResult.ErrorMessages.AddRange(command.ErrorMessages);
					}
				}

				if(actionResult.ErrorMessages.Any())
					result.Add(actionResult);
			}

			return result;
		}

		private bool agentScheduleIsWriteProtected(DateOnly date, IPerson agent)
		{
			return !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				&& agent.PersonWriteProtection.IsWriteProtected(date);
		}

		private bool checkPermissionFn(Dictionary<string, string> permissions, DateOnly date, IPerson agent, IList<string> messages)
		{
			var newMessages = new List<string>();

			if (agentScheduleIsWriteProtected(date, agent))
			{
				newMessages.Add(Resources.WriteProtectSchedule);
			}

			newMessages.AddRange(
				from permission in permissions.Keys
				where !_permissionProvider.HasPersonPermission(permission, date, agent)
				select permissions[permission]);

			messages.AddRange(newMessages);
			return !newMessages.Any();
		}
	}

	public interface ITeamScheduleCommandHandlingProvider
	{
		List<ActionResult> AddActivity(AddActivityFormData formData);
		List<ActionResult> AddPersonalActivity(AddPersonalActivityFormData formData);
		IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds);
		List<ActionResult> RemoveActivity(RemoveActivityFormData input);
		List<ActionResult> MoveActivity(MoveActivityFormData input);
		List<ActionResult> BackoutScheduleChange(BackoutScheduleChangeFormData input);
		List<ActionResult> ChangeShiftCategory(ChangeShiftCategoryFormData input);
		IList<ActionResult> MoveNonoverwritableLayers(MoveNonoverwritableLayersFormData input);
	}
}