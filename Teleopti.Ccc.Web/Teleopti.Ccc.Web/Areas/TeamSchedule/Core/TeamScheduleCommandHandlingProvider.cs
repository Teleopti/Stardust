using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleCommandHandlingProvider : ITeamScheduleCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IChangeActivityTypeFormValidator _changeActivityTypeFormValidator;
		private readonly IDictionary<string, Func<string>> _permissionDic = new Dictionary<string, Func<string>>
		{
			{DefinedRaptorApplicationFunctionPaths.RemoveAbsence,()=> Resources.NoPermissionRemoveAgentAbsence},
			{ DefinedRaptorApplicationFunctionPaths.EditShiftCategory,()=> Resources.NoPermissionToEditShiftCategory },
			{ DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity,()=> Resources.NoPermissionToMoveInvalidOverlappedActivity },
			{ DefinedRaptorApplicationFunctionPaths.RemoveShift,()=> Resources.NoPermissionRemovingShift },
			{ DefinedRaptorApplicationFunctionPaths.AddDayOff, ()=>Resources.NoPermissionAddDayOff },
			{ DefinedRaptorApplicationFunctionPaths.RemoveDayOff, ()=>Resources.NoPermissionRemoveDayOff }
		};

		public TeamScheduleCommandHandlingProvider(
			ICommandDispatcher commandDispatcher,
			ILoggedOnUser loggedOnUser,
			IPersonRepository personRepository,
			IPermissionProvider permissionProvider,
			IDayOffTemplateRepository dayOffTemplateRepository,
			ICurrentScenario currentScenario,
			IScheduleStorage scheduleStorage,
			IChangeActivityTypeFormValidator changeActivityTypeFormValidator)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_currentScenario = currentScenario;
			_scheduleStorage = scheduleStorage;
			_changeActivityTypeFormValidator = changeActivityTypeFormValidator;
		}

		public IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds)
		{
			var agents = _personRepository.FindPeople(agentIds);
			return agents.Where(agent => agentScheduleIsWriteProtected(date, agent)).Select(x => x.Id.GetValueOrDefault()).ToList();
		}

		public List<ActionResult> RemoveAbsence(RemovePersonAbsenceForm input)
		{
			var result = new List<ActionResult>();

			var people = _personRepository.FindPeople(input.SelectedPersonAbsences.Select(p => p.PersonId)).ToDictionary(p => p.Id.GetValueOrDefault());
			foreach (var selectedPersonAbsence in input.SelectedPersonAbsences)
			{
				var absenceDateGroups = selectedPersonAbsence.AbsenceDates.GroupBy(absenceDate => absenceDate.Date, absenceDate => absenceDate.PersonAbsenceId);
				var person = people[selectedPersonAbsence.PersonId];

				foreach (var absenceGroup in absenceDateGroups)
				{
					var actionResult = new ActionResult(selectedPersonAbsence.PersonId);
					var date = absenceGroup.Key;
					var loadOptions = new ScheduleDictionaryLoadOptions(false, false);
					var period = new DateOnlyPeriod(date.AddDays(-1), date);
					var currentScenario = _currentScenario.Current();
					var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, loadOptions, period, currentScenario);
					var scheduleRange = dictionary?[person];

					if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.RemoveAbsence, date, person, actionResult.ErrorMessages))
					{
						var command = new RemoveSelectedPersonAbsenceCommand
						{
							Person = person,
							Date = date,
							TrackedCommandInfo =
								input.TrackedCommandInfo ?? new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value },
							ScheduleRange = scheduleRange
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

		public List<ActionResult> BackoutScheduleChange(BackoutScheduleChangeFormData input)
		{
			var result = new List<ActionResult>();

			var personDateGroups = input.PersonDates.GroupBy(personDate => personDate.PersonId, personDate => personDate.Date);

			foreach (var dates in personDateGroups)
			{
				var personId = dates.Key;
				var person = _personRepository.Get(personId);
				var actionResult = new ActionResult(personId);
				if (dates.All(date =>
									{
										if (agentScheduleIsWriteProtected(date, person))
										{
											actionResult.ErrorMessages.Add(Resources.WriteProtectSchedule);
											return false;
										}
										return true;
									}))
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

		public List<ActionResult> ChangeShiftCategory(ChangeShiftCategoryFormData input)
		{
			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult(person.Id.GetValueOrDefault());

				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.EditShiftCategory, input.Date, person, actionResult.ErrorMessages))
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
			var result = new List<ActionResult>();
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult(person.Id.GetValueOrDefault());

				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, input.Date, person, actionResult.ErrorMessages))
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
			var actionResult = new ActionResult(input.PersonId);

			var retResult = new List<ActionResult>();
			var person = _personRepository.Get(input.PersonId);

			if (agentScheduleIsWriteProtected(input.SelectedDate, person))
			{
				actionResult.ErrorMessages.Add(Resources.WriteProtectSchedule);
				retResult.Add(actionResult);
				return retResult;
			}

			var command = new EditScheduleNoteCommand
			{
				PersonId = input.PersonId,
				Date = input.SelectedDate,
				InternalNote = input.InternalNote,
				PublicNote = input.PublicNote
			};
			_commandDispatcher.Execute(command);


			if (command.ErrorMessages != null && command.ErrorMessages.Any())
				retResult.Add(new ActionResult(input.PersonId)
				{
					ErrorMessages = command.ErrorMessages
				});

			return retResult;
		}

		public IList<ActionResult> AddDayOff(AddDayOffFormData input)
		{
			var result = new List<ActionResult>();
			if (!validateAddDayOffInput(input))
			{
				result.Add(new ActionResult()
				{
					ErrorMessages = new List<string> { Resources.InvalidInput }
				});
				return result;
			}
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult(person.Id.GetValueOrDefault());
				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.AddDayOff, input.StartDate, person, actionResult.ErrorMessages))
				{
					var command = new AddDayOffCommand
					{
						Person = person,
						StartDate = input.StartDate,
						EndDate = input.EndDate,
						Template = input.Template,
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

		public IList<ActionResult> RemoveDayOff(RemoveDayOffFormData input)
		{
			var result = new List<ActionResult>();
			if (!validateRemoveDayOffInput(input))
			{
				result.Add(new ActionResult
				{
					ErrorMessages = new List<String> { Resources.InvalidInput }
				});
				return result;
			}
			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult(person.Id.Value);
				if (checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.RemoveDayOff, input.Date, person, actionResult.ErrorMessages))
				{
					var command = new RemoveDayOffCommand
					{
						Date = input.Date,
						Person = person,
						TrackedCommandInfo = new TrackedCommandInfo { TrackId = input.TrackedCommandInfo.TrackId }
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

		public IList<ActionResult> RemoveShift(RemoveShiftFormData input)
		{
			var result = new List<ActionResult>();
			if (!validateRemoveShiftInput(input))
			{
				result.Add(new ActionResult
				{
					ErrorMessages = new List<string> { Resources.InvalidInput }
				});
				return result;
			}

			var people = _personRepository.FindPeople(input.PersonIds);
			foreach (var person in people)
			{
				var actionResult = new ActionResult(person.Id.GetValueOrDefault());
				if (!checkFunctionPermission(DefinedRaptorApplicationFunctionPaths.RemoveShift, input.Date, person,
					actionResult.ErrorMessages))
				{
					result.Add(actionResult);
					continue;
				}
				var command = new RemoveShiftCommand
				{
					Date = input.Date,
					Person = person,
					TrackedCommandInfo = new TrackedCommandInfo { TrackId = input.TrackedCommandInfo.TrackId }
				};
				_commandDispatcher.Execute(command);

				if (command.ErrorMessages != null && command.ErrorMessages.Any())
				{
					actionResult.ErrorMessages.AddRange(command.ErrorMessages);
				}
			}
			return result;
		}

		public IList<ActionResult> ChangeActivityType(ChangeActivityTypeFormData input)
		{
			var result = new List<ActionResult>();

			var validateResult = _changeActivityTypeFormValidator.Validate(input);

			if (!validateResult.IsValid)
			{
				result.Add(new ActionResult(input.PersonId)
				{
					ErrorMessages = validateResult.ErrorMessages
				});
				return result;
			}

			var cmds = getChangeActivityTypeCommands(input, validateResult.Person, validateResult.ScheduleDictionary, validateResult.Activities);

			var mulitpleCommand = new MultipleChangeScheduleCommand
			{
				Commands = cmds,
				Person = validateResult.Person,
				Date = new DateOnly(input.Date),
				ScheduleDictionary = validateResult.ScheduleDictionary,
				TrackedCommandInfo = input.TrackedCommandInfo
			};

			_commandDispatcher.Execute(mulitpleCommand);

			return result;
		}
		private bool validateRemoveShiftInput(RemoveShiftFormData input)
		{
			if (input.Date.Date == DateTime.MinValue || input.PersonIds == null || !input.PersonIds.Any())
			{
				return false;
			}
			return true;
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
				newMessages.Add(_permissionDic[path]());
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


		private bool validateAddDayOffInput(AddDayOffFormData input)
		{
			if (input.StartDate == null || input.EndDate == null || input.StartDate > input.EndDate)
			{
				return false;
			}
			if (input.PersonIds == null || !input.PersonIds.Any())
			{
				return false;
			}

			if (input.TemplateId == null
				|| (input.Template = _dayOffTemplateRepository.Get(input.TemplateId)) == null)
			{
				return false;
			}

			return true;
		}

		private bool validateRemoveDayOffInput(RemoveDayOffFormData input)
		{
			if (input.Date.Date == DateTime.MinValue || input.PersonIds == null || !input.PersonIds.Any())
				return false;

			return true;
		}

		private IList<IScheduleCommand> getChangeActivityTypeCommands(
			ChangeActivityTypeFormData input,
			IPerson person,
			IScheduleDictionary scheduleDictionary,
			IDictionary<Guid, IActivity> activities)
		{
			var commands = new List<IScheduleCommand>();
			var date = new DateOnly(input.Date);
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var shiftLayers = scheduleDictionary[person].ScheduledDay(date).PersonAssignment().ShiftLayers.ToDictionary(sl => sl.Id);

			foreach (var layer in input.Layers)
			{
				foreach (var layerId in layer.ShiftLayerIds)
				{
					var shiftLayer = shiftLayers[layerId];
					if (layer.IsNew)
					{
						var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(layer.StartTime.Value, timezone),
								TimeZoneHelper.ConvertToUtc(layer.EndTime.Value, timezone));

						switch (shiftLayer.GetType().Name)
						{
							case nameof(PersonalShiftLayer):
								commands.Add(new AddPersonalActivityCommandSimply
								{
									Period = period,
									Activity = activities[layer.ActivityId]
								});
								break;
							case nameof(OvertimeShiftLayer):
								var overtimeShiftLayer = shiftLayer as OvertimeShiftLayer;

								commands.Add(new AddOvertimeActivityCommandSimply
								{
									Activity = activities[layer.ActivityId],
									Period = period,
									MultiplicatorDefinitionSet = overtimeShiftLayer.DefinitionSet
								});
								break;
							default:
								commands.Add(new AddActivityCommandSimply
								{
									Period = period,
									Activity = activities[layer.ActivityId]
								});
								break;
						}

						continue;
					}

					commands.Add(new ChangeActivityTypeCommand
					{
						Activity = activities[layer.ActivityId],
						ShiftLayer = shiftLayers[layerId]
					});

				}
			}
			return commands;
		}


	}
}