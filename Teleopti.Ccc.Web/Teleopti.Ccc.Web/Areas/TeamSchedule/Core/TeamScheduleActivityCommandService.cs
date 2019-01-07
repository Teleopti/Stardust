using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

using System;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleActivityCommandService
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IMoveShiftLayerCommandHelper _helper;
		private readonly IDictionary<string, Func<string>> _permissionResourceDic = new Dictionary<string, Func<string>>
		{
			{DefinedRaptorApplicationFunctionPaths.AddActivity,  ()=> Resources.NoPermissionAddAgentActivity},
			{DefinedRaptorApplicationFunctionPaths.AddPersonalActivity,  ()=> Resources.NoPermissionAddPersonalActivity},
			{DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity,  ()=> Resources.NoPermissionAddOvertimeActivity},
			{DefinedRaptorApplicationFunctionPaths.RemoveOvertime,  ()=>Resources.NoPermissionRemoveOvertimeActivity },
			{DefinedRaptorApplicationFunctionPaths.RemoveActivity, ()=> Resources.NoPermissionRemoveAgentActivity },
			{DefinedRaptorApplicationFunctionPaths.MoveActivity,  ()=>Resources.NoPermissionMoveAgentActivity}
		};

		public TeamScheduleActivityCommandService(
			ICommandDispatcher commandDispatcher,
			ILoggedOnUser loggedOnUser,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IMoveShiftLayerCommandHelper helper)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_helper = helper;
		}

		public List<ActionResult> AddActivity(AddActivityFormData input)
		{
			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonDates.Select(x => x.PersonId)).ToLookup(p => p.Id);

			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult(personId);
				var person = people[personId].SingleOrDefault();

				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.AddActivity, date, person, actionResult.ErrorMessages))
				{
					var command = new AddActivityCommand
					{
						Person = person,
						ActivityId = input.ActivityId,
						Date = date,
						StartTime = input.StartTime,
						EndTime = input.EndTime,
						MoveConflictLayerAllowed = input.MoveConflictLayerAllowed,
						TrackedCommandInfo =
							input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
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
			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonDates.Select(x => x.PersonId)).ToLookup(p => p.Id);
			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult(personId);
				var person = people[personId].SingleOrDefault();

				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, date, person, actionResult.ErrorMessages))
				{
					var command = new AddPersonalActivityCommand
					{
						Person = person,
						PersonalActivityId = input.ActivityId,
						Date = date,
						StartTime = input.StartTime,
						EndTime = input.EndTime,
						TrackedCommandInfo =
							input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
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

		public IList<ActionResult> AddOvertimeActivity(AddOvertimeActivityForm input)
		{
			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonDates.Select(x => x.PersonId)).ToLookup(p => p.Id);

			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult(personId);
				var person = people[personId].SingleOrDefault();

				var currentUser = _loggedOnUser.CurrentUser();
				var userTimezone = currentUser.PermissionInformation.DefaultTimeZone();
				var startDateTimeUtc = TimeZoneHelper.ConvertToUtc(input.StartDateTime, userTimezone);
				var endDateTimeUtc = TimeZoneHelper.ConvertToUtc(input.EndDateTime, userTimezone);

				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity, date, person, actionResult.ErrorMessages))
				{
					var command = new AddOvertimeActivityCommand
					{
						Person = person,
						ActivityId = input.ActivityId,
						Date = date,
						Period = new DateTimePeriod(startDateTimeUtc, endDateTimeUtc),
						MultiplicatorDefinitionSetId = input.MultiplicatorDefinitionSetId,
						TrackedCommandInfo =
							input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = currentUser.Id.Value }
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

		public List<ActionResult> RemoveActivity(RemoveActivityFormData input)
		{
			var result = new List<ActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);

				foreach (var shiftLayerDate in personActivity.ShiftLayers)
				{
					var errorMessages = new List<string>();
					var hasPermission = shiftLayerDate.IsOvertime
						? checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.RemoveOvertime, shiftLayerDate.Date, person,
							errorMessages)
						: checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.RemoveActivity, shiftLayerDate.Date, person,
							errorMessages);
					if (hasPermission)
					{
						var command = new RemoveActivityCommand
						{
							PersonId = personActivity.PersonId,
							ShiftLayerId = shiftLayerDate.ShiftLayerId,
							Date = shiftLayerDate.Date,
							TrackedCommandInfo =
								input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
						};

						_commandDispatcher.Execute(command);
						if (command.ErrorMessages != null && command.ErrorMessages.Any())
						{
							errorMessages.AddRange(command.ErrorMessages);
						}
					}
					if (errorMessages.Any())
						result.Add(new ActionResult(personActivity.PersonId)
						{
							ErrorMessages = errorMessages
						});
				}


			}

			return result;
		}

		public List<ActionResult> MoveActivity(MoveActivityFormData input)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var userTimezone = currentUser.PermissionInformation.DefaultTimeZone();
			var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.StartTime, userTimezone);
			var result = new List<ActionResult>();

			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);
				var personError = new ActionResult(person.Id.GetValueOrDefault());

				if (personActivity.ShiftLayerIds.Count != 1) {
					personError.ErrorMessages.Add(Resources.CanNotMoveMultipleActivitiesForSelectedAgents);
					result.Add(personError);
					continue;
				}
				if (!_helper.CheckPermission(personActivity.ShiftLayerIds, person, personActivity.Date, out var permissionErrors))
				{
					personError.ErrorMessages.AddRange(permissionErrors);
					result.Add(personError);
					continue;
				}
				var layerToMoveTimeMap = _helper.GetCorrectNewStartForLayersForPerson(person, personActivity.Date, personActivity.ShiftLayerIds,
					newStartTimeInUtc);
				if (personActivity.ShiftLayerIds.Any(x => !layerToMoveTimeMap.ContainsKey(x)))
				{
					personError.ErrorMessages.Add(Resources.NoShiftsFound);
					result.Add(personError);
					continue;
				}

				if (_helper.ValidateLayerMoveToTime(layerToMoveTimeMap, person, personActivity.Date))
				{
					layerToMoveTimeMap
						.ForEach(
							pl =>
							{
								var command = new MoveShiftLayerCommand
								{
									AgentId = personActivity.PersonId,
									NewStartTimeInUtc = pl.Value,
									ScheduleDate = personActivity.Date,
									ShiftLayerId = pl.Key,
									TrackedCommandInfo =
										input.TrackedCommandInfo ??
										new TrackedCommandInfo { OperatedPersonId = currentUser.Id.GetValueOrDefault() }
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

		public IList<ActionResult> MoveShift(MoveShiftForm input)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var userTimezone = currentUser.PermissionInformation.DefaultTimeZone();
			var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.NewShiftStart, userTimezone);
			var result = new List<ActionResult>();

			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var personError = new ActionResult(person.Id.GetValueOrDefault());
				if (!checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.MoveActivity, input.Date, person, personError.ErrorMessages))
				{
					result.Add(personError);
					continue;
				}

				var command = new MoveShiftCommand
				{
					PersonId = person.Id.GetValueOrDefault(),
					ScheduleDate = input.Date,
					NewStartTimeInUtc = newStartTimeInUtc,
					TrackedCommandInfo =
						input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = currentUser.Id.Value }
				};
				_commandDispatcher.Execute(command);
				if (command.ErrorMessages != null && command.ErrorMessages.Any())
				{
					personError.ErrorMessages.AddRange(command.ErrorMessages);
				}
				if (personError.ErrorMessages.Any())
					result.Add(personError);
			}

			return result;
		}

		private bool agentScheduleIsWriteProtected(DateOnly date, IPerson agent)
		{
			return !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				&& agent.PersonWriteProtection.IsWriteProtected(date);
		}

		private bool checkFunctionPermission(string path, DateOnly date, IPerson agent, IList<string> messages)
		{
			var newMessages = new List<string>();

			if (agentScheduleIsWriteProtected(date, agent))
			{
				newMessages.Add(Resources.WriteProtectSchedule);
			}
			if (!_permissionProvider.HasPersonPermission(path, date, agent))
			{
				
				newMessages.Add(_permissionResourceDic[path]());
			}
			if (!_permissionProvider.IsPersonSchedulePublished(date, agent) &&
				!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, date,
					agent))
			{
				newMessages.Add(Resources.NoPermissionToEditUnpublishedSchedule);
			}

			messages.AddRange(newMessages);
			return !newMessages.Any();
		}
	}
}