using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
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

			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();
				actionResult.WarningMessages = new List<string>();

				if (checkPermissionFn(permissions, date, person, actionResult.ErrorMessages))
				{
					var command = new AddActivityCommand
					{
						PersonId = personId,
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
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.AddPersonalActivity,  Resources.NoPermissionAddPersonalActivity}
			};

			var result = new List<ActionResult>();
			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();

				if (checkPermissionFn(permissions, date, person, actionResult.ErrorMessages))
				{
					var command = new AddPersonalActivityCommand
					{
						PersonId = personId,
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
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity,  Resources.NoPermissionAddOvertimeActivity}
			};

			var result = new List<ActionResult>();

			foreach (var personDate in input.PersonDates)
			{
				var personId = personDate.PersonId;
				var date = personDate.Date;

				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();

				var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				var startDateTimeUtc = TimeZoneHelper.ConvertToUtc(input.StartDateTime, userTimezone);
				var endDateTimeUtc = TimeZoneHelper.ConvertToUtc(input.EndDateTime, userTimezone);

				if (checkPermissionFn(permissions, date, person, actionResult.ErrorMessages))
				{
					var command = new AddOvertimeActivityCommand
					{
						PersonId = personId,
						ActivityId = input.ActivityId,
						Date = date,
						Period = new DateTimePeriod(startDateTimeUtc, endDateTimeUtc),
						MultiplicatorDefinitionSetId = input.MultiplicatorDefinitionSetId,
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

		public IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds)
		{
			var agents = _personRepository.FindPeople(agentIds);
			return agents.Where(agent => agentScheduleIsWriteProtected(date, agent)).Select(x => x.Id.GetValueOrDefault()).ToList();
		}

		public List<ActionResult> RemoveActivity(RemoveActivityFormData input)
		{
			var result = new List<ActionResult>();
			foreach (var personActivity in input.PersonActivities)
			{
				var actionResult = new ActionResult();
				var person = _personRepository.Get(personActivity.PersonId);
				actionResult.PersonId = personActivity.PersonId;
				actionResult.ErrorMessages = new List<string>();
				foreach (var shiftLayerDate in personActivity.ShiftLayers)
				{
					if (checkRemoveActivityPermission(shiftLayerDate, person, actionResult.ErrorMessages))
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
							actionResult.ErrorMessages.AddRange(command.ErrorMessages);
						}
					}
				}

				actionResult.ErrorMessages.ForEach(e =>
				{
					result.Add(new ActionResult
					{
						ErrorMessages = new List<string> { e },
						PersonId = personActivity.PersonId
					});
				});
			}

			return result;
		}

		public List<ActionResult> RemoveAbsence(RemovePersonAbsenceForm input)
		{
			var permissions = new Dictionary<string, string>
			{
				{  DefinedRaptorApplicationFunctionPaths.RemoveAbsence, Resources.NoPermissionRemoveAgentAbsence}
			};

			var result = new List<ActionResult>();

			var people = _personRepository.FindPeople(input.SelectedPersonAbsences.Select(p => p.PersonId)).ToDictionary(p => p.Id.GetValueOrDefault());
			foreach (var selectedPersonAbsence in input.SelectedPersonAbsences)
			{
				var absenceDateGroups = selectedPersonAbsence.AbsenceDates.GroupBy(absenceDate => absenceDate.Date, absenceDate => absenceDate.PersonAbsenceId);
				var person = people[selectedPersonAbsence.PersonId];

				foreach (var absenceGroup in absenceDateGroups)
				{
					var actionResult = new ActionResult();
					var date = absenceGroup.Key;

					actionResult.PersonId = selectedPersonAbsence.PersonId;
					actionResult.ErrorMessages = new List<string>();

					if (checkPermissionFn(permissions, date, person, actionResult.ErrorMessages))
					{
						var command = new RemoveSelectedPersonAbsenceCommand
						{
							PersonId = selectedPersonAbsence.PersonId,
							Date = date,
							TrackedCommandInfo =
								input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
						};

						foreach (var personAbsenceId in absenceGroup)
						{
							command.PersonAbsenceId = personAbsenceId;
							_commandDispatcher.Execute(command);
						}

						if (command.ErrorMessages != null && command.ErrorMessages.Any())
						{
							actionResult.ErrorMessages.AddRange(command.ErrorMessages);
						}
					}

					if (actionResult.ErrorMessages.Any())
						result.Add(actionResult);
				}

			}

			return result;
		}

		public IList<ActionResult> MoveShift(MoveShiftForm input)
		{
			var permission = new Dictionary<string, string>
			{
				{DefinedRaptorApplicationFunctionPaths.MoveActivity, Resources.NoPermissionMoveAgentActivity}
			};
			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.NewShiftStart, userTimezone);
			var result = new List<ActionResult>();

			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var personError = new ActionResult { PersonId = person.Id.GetValueOrDefault(), ErrorMessages = new List<string>() };
				if (!checkPermissionFn(permission, input.Date, person, personError.ErrorMessages))
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
							input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
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

		public List<ActionResult> BackoutScheduleChange(BackoutScheduleChangeFormData input)
		{
			var permissions = new Dictionary<string, string>();

			var result = new List<ActionResult>();

			var personDateGroups = input.PersonDates.GroupBy(personDate => personDate.PersonId, personDate => personDate.Date);

			foreach (var dates in personDateGroups)
			{
				var personId = dates.Key;

				var actionResult = new ActionResult();
				var person = _personRepository.Get(personId);
				actionResult.PersonId = personId;
				actionResult.ErrorMessages = new List<string>();
				if (dates.All(date => checkPermissionFn(permissions, date, person, actionResult.ErrorMessages)))
				{
					var command = new BackoutScheduleChangeCommand
					{
						PersonId = personId,
						Dates = dates.ToArray(),
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


		public List<ActionResult> MoveActivity(MoveActivityFormData input)
		{

			var userTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.StartTime, userTimezone);
			var result = new List<ActionResult>();

			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);
				var personError = new ActionResult { PersonId = person.Id.GetValueOrDefault(), ErrorMessages = new List<string>() };

				List<string> permissionErrors;
				if (!_helper.CheckPermission(personActivity.ShiftLayerIds, person, personActivity.Date, out permissionErrors))
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
										new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault() }
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
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult();
				actionResult.PersonId = person.Id.GetValueOrDefault();
				actionResult.ErrorMessages = new List<string>();

				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new ChangeShiftCategoryCommand
					{
						PersonId = person.Id.GetValueOrDefault(),
						Date = input.Date,
						ShiftCategoryId = input.ShiftCategoryId,
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

		public IList<ActionResult> MoveNonoverwritableLayers(MoveNonoverwritableLayersFormData input)
		{
			var permissions = new Dictionary<string, string>
			{
				{ DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, Resources.NoPermissionToMoveInvalidOverlappedActivity }
			};

			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult();
				actionResult.PersonId = person.Id.GetValueOrDefault();
				actionResult.ErrorMessages = new List<string>();

				if (checkPermissionFn(permissions, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new FixNotOverwriteLayerCommand
					{
						PersonId = person.Id.GetValueOrDefault(),
						Date = input.Date,
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

		public IList<ActionResult> EditScheduleNote(EditScheduleNoteFormData input)
		{
			var actionResult = new ActionResult
			{
				PersonId = input.PersonId
			};

			var retResult = new List<ActionResult>();
			var person = _personRepository.Get(input.PersonId);

			if (agentScheduleIsWriteProtected(input.SelectedDate, person))
			{
				actionResult.ErrorMessages = new List<string>
				{
					Resources.WriteProtectSchedule
				};
				retResult.Add(actionResult);
				return retResult;
			}

			var command = new EditScheduleNoteCommand
			{
				PersonId = input.PersonId,
				Date = input.SelectedDate,
				InternalNote = input.InternalNote
			};
			_commandDispatcher.Execute(command);


			if (command.ErrorMessages != null && command.ErrorMessages.Any())
				retResult.Add(new ActionResult
				{
					PersonId = input.PersonId,
					ErrorMessages = command.ErrorMessages
				});

			return retResult;
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
		private bool checkRemoveActivityPermission(ShiftLayerDate layerDate, IPerson agent, IList<string> messages)
		{
			var date = layerDate.Date;
			var newMessages = new List<string>();

			if (agentScheduleIsWriteProtected(date, agent))
			{
				newMessages.Add(Resources.WriteProtectSchedule);
			}

			if (layerDate.IsOvertime && !_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.RemoveOvertime, date, agent))
			{
				newMessages.Add(Resources.NoPermissionRemoveOvertimeActivity);
			}
			else if (!layerDate.IsOvertime && !_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.RemoveActivity, date, agent))
			{
				newMessages.Add(Resources.NoPermissionRemoveAgentActivity);
			}

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
		IList<ActionResult> EditScheduleNote(EditScheduleNoteFormData input);
		List<ActionResult> RemoveAbsence(RemovePersonAbsenceForm input);
		IList<ActionResult> MoveShift(MoveShiftForm input);
		IList<ActionResult> AddOvertimeActivity(AddOvertimeActivityForm input);
	}
}