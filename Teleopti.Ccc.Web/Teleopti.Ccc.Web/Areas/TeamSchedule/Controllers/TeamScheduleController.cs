﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)]
	public class TeamScheduleController : ApiController
	{
		private readonly IAuthorization _authorization;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IAbsencePersister _absencePersister;
		private readonly ISettingsPersisterAndProvider<AgentsPerPageSetting> _agentsPerPagePersisterAndProvider;
		private readonly ISwapMainShiftForTwoPersonsCommandHandler _swapMainShiftForTwoPersonsHandler;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISearchTermParser _parser;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory,
			ILoggedOnUser loggonUser,
			IPersonAbsenceRepository personAbsenceRepository,
			IAuthorization authorization, IAbsencePersister absencePersister,
			ISettingsPersisterAndProvider<AgentsPerPageSetting> agentsPerPagePersisterAndProvider,
			ISwapMainShiftForTwoPersonsCommandHandler swapMainShiftForTwoPersonsHandler,
			ICommandDispatcher commandDispatcher,
			ICurrentScenario currentScenario, 
			ISearchTermParser parser)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_loggonUser = loggonUser;
			_personAbsenceRepository = personAbsenceRepository;
			_authorization = authorization;
			_absencePersister = absencePersister;
			_agentsPerPagePersisterAndProvider = agentsPerPagePersisterAndProvider;
			_swapMainShiftForTwoPersonsHandler = swapMainShiftForTwoPersonsHandler;
			_commandDispatcher = commandDispatcher;
			_currentScenario = currentScenario;
			_parser = parser;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GetPermissions")]
		public virtual JsonResult<PermissionsViewModel> GetPermissions()
		{
			var permissions = new PermissionsViewModel
			{
				IsAddFullDayAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				IsAddIntradayAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence),
				IsSwapShiftsAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SwapShifts),
				IsRemoveAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveAbsence),
				IsModifyScheduleAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule),
				HasAddingActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity),
				HasAddingPersonalActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity),
				HasRemoveActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveActivity),
				HasMoveActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity),
				HasMoveInvalidOverlappedActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity),
				HasEditShiftCategoryPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.EditShiftCategory)
			};

			return Json(permissions);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/SearchSchedules")]
		public virtual JsonResult<GroupScheduleViewModel> SearchSchedules(string keyword, DateTime date, int pageSize, int currentPageIndex, bool isOnlyAbsences)
		{
			var currentDate = new DateOnly(date);
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);

			if (string.IsNullOrEmpty(keyword) && myTeam == null)
			{
				return
					Json(new GroupScheduleViewModel {Schedules = new List<GroupScheduleShiftViewModel>(), Total = 0, Keyword = ""});
			}

			var criteriaDictionary = _parser.Parse(keyword, currentDate);

			var result =
				_teamScheduleViewModelFactory.CreateViewModel(criteriaDictionary, currentDate, pageSize, currentPageIndex, isOnlyAbsences);
			result.Keyword = _parser.Keyword(keyword, currentDate);

			return Json(result);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/SearchWeekSchedules")]
		public virtual JsonResult<GroupWeekScheduleViewModel> SearchWeekSchedules(SearchWeekSchedulesFormData input)
		{
			var currentDate = new DateOnly(input.Date);
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);

			if(string.IsNullOrEmpty(input.Keyword) && myTeam == null)
			{
				return
					Json(new GroupWeekScheduleViewModel { PersonWeekSchedules = new List<PersonWeekScheduleViewModel>(),Total = 0,Keyword = "" });
			}

			var criteriaDictionary = _parser.Parse(input.Keyword, currentDate);

			var result =
				_teamScheduleViewModelFactory.CreateWeekScheduleViewModel(criteriaDictionary,currentDate, input.PageSize, input.CurrentPageIndex);
			result.Keyword = _parser.Keyword(input.Keyword, currentDate);

			return Json(result);
		}


		[UnitOfWork, HttpPost, Route("api/TeamSchedule/GetSchedules")]
		public virtual GroupScheduleViewModel GetSchedulesForPeople(GetSchedulesForPeopleFormData form)
		{
			return getSchedulesForPeople( form.PersonIds, form.Date);
		}

		[HttpPost, UnitOfWork, AddFullDayAbsencePermission, Route("api/TeamSchedule/AddFullDayAbsence")]
		public virtual JsonResult<List<ActionResult>> AddFullDayAbsence(FullDayAbsenceForm form)
		{
			setTrackedCommandInfo(form.TrackedCommandInfo);
			var failResults = new List<ActionResult>();
			foreach (var personId in form.PersonIds)
			{
				var command = new AddFullDayAbsenceCommand
				{
					PersonId = personId,
					AbsenceId = form.AbsenceId,
					StartDate = form.Start,
					EndDate = form.End,
					TrackedCommandInfo = form.TrackedCommandInfo
				};
				var persistResult = _absencePersister.PersistFullDayAbsence(command);
				if (persistResult != null)
				{
					failResults.Add(persistResult);
				}
			}

			return Json(failResults);
		}

		[HttpPost, UnitOfWork, AddIntradayAbsencePermission, Route("api/TeamSchedule/AddIntradayAbsence")]
		public virtual IHttpActionResult AddIntradayAbsence(IntradayAbsenceForm form)
		{
			if (!form.IsValid())
			{
				return BadRequest(Resources.EndTimeMustBeGreaterOrEqualToStartTime);
			}

			setTrackedCommandInfo(form.TrackedCommandInfo);
			var failResults = new List<ActionResult>();
			foreach (var personId in form.PersonIds)
			{
				var command = new AddIntradayAbsenceCommand
				{
					PersonId = personId,
					AbsenceId = form.AbsenceId,
					StartTime = form.Start,
					EndTime = form.End,
					TrackedCommandInfo = form.TrackedCommandInfo
				};
				var persistResult = _absencePersister.PersistIntradayAbsence(command);
				if (persistResult != null)
				{
					failResults.Add(persistResult);
				}
			}

			return Ok(failResults);
		}

		[HttpPost, UnitOfWork, Route("api/TeamSchedule/RemoveAbsence")]
		public virtual IHttpActionResult RemoveAbsence(RemovePersonAbsenceForm command)
		{
			setTrackedCommandInfo(command.TrackedCommandInfo);
			var errors = new List<ActionResult>();
			foreach (var personAbsence in command.SelectedPersonAbsences)
			{
				var personAbsencesForRemove = _personAbsenceRepository.Find(personAbsence.PersonAbsenceIds, _currentScenario.Current());
				var result = command.RemoveEntireCrossDayAbsence
				? removeEntirePersonAbsence(command.ScheduleDate, personAbsencesForRemove, command.TrackedCommandInfo)
				: removePartPersonAbsence(command.ScheduleDate, personAbsencesForRemove, command.ScheduleDate,
					command.TrackedCommandInfo);
				errors.AddRange(result);
			}

			return Ok(errors);
		}

		[HttpPost, UnitOfWork, Route("api/TeamSchedule/UpdateAgentsPerPage")]
		public virtual IHttpActionResult UpdateAgentsPerPageSetting(int agents)
		{
			var agentsPerPageSetting = new AgentsPerPageSetting
			{
				AgentsPerPage = agents
			};
			_agentsPerPagePersisterAndProvider.Persist(agentsPerPageSetting);

			return Ok();
		}

		[HttpPost, UnitOfWork, Route("api/TeamSchedule/GetAgentsPerPage")]
		public virtual JsonResult<AgentsPerPageSettingViewModel> GetAgentsPerPageSetting()
		{
			var agentsPerPageSetting = _agentsPerPagePersisterAndProvider.GetByOwner(_loggonUser.CurrentUser());
			return Json(new AgentsPerPageSettingViewModel {Agents = agentsPerPageSetting.AgentsPerPage});
		}

		[HttpPost, UnitOfWork, SwapShiftPermission, Route("api/TeamSchedule/SwapShifts")]
		public virtual IHttpActionResult SwapShifts(SwapMainShiftForTwoPersonsCommand command)
		{
			setTrackedCommandInfo(command.TrackedCommandInfo);
			var failResults = _swapMainShiftForTwoPersonsHandler.SwapShifts(command);
			return Json(failResults);
		}

		private void setTrackedCommandInfo(TrackedCommandInfo commandInfo)
		{
			if (commandInfo == null) return;

			var userId = _loggonUser.CurrentUser().Id;
			if (userId != null)
			{
				commandInfo.OperatedPersonId = userId.Value;
			}
		}

		private GroupScheduleViewModel getSchedulesForPeople(IEnumerable<Guid> personIds, DateTime date)
		{
			var scheduleDateOnly = new DateOnly(date);
			return _teamScheduleViewModelFactory.CreateViewModelForPeople(personIds.ToArray(), scheduleDateOnly);
		}

		private IEnumerable<ActionResult> removeEntirePersonAbsence(DateTime scheduleDate,
			IEnumerable<IPersonAbsence> personAbsencesForRemove, TrackedCommandInfo trackInfo)
		{
			var personAbsenceGroups = personAbsencesForRemove
				.GroupBy(pa => pa.Person)
				.Select(group => new
				{
					Person = group.Key,
					PersonAbsences = group.Select(x=>x)
				});
			
			var failResults = new List<ActionResult>();
			foreach (var personAbsenceGroup in personAbsenceGroups)
			{
				var cmd = new RemovePersonAbsenceCommand
				{
					Person = personAbsenceGroup.Person,
					ScheduleDate = scheduleDate,
					PersonAbsences = personAbsenceGroup.PersonAbsences,
					TrackedCommandInfo = trackInfo
				};

				_commandDispatcher.Execute(cmd);

				if (cmd.Errors == null) continue;

				appendErrorMessage(failResults, cmd.Errors);
			}
			return failResults;
		}

		private IEnumerable<ActionResult> removePartPersonAbsence(DateTime scheduleDate,
			IEnumerable<IPersonAbsence> personAbsencesForRemove,
			DateTime dateToRemove, TrackedCommandInfo trackInfo)
		{
			var personAbsenceGroups = personAbsencesForRemove
				.GroupBy(pa => pa.Person)
				.Select(group => new
				{
					Person = group.Key,
					PersonAbsences = group.Select(x => x)
				});

			var scheduleDateInUtc = TimeZoneInfo.ConvertTimeToUtc(dateToRemove,
				_loggonUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			var periodToRemove = new DateTimePeriod(scheduleDateInUtc, scheduleDateInUtc.AddDays(1));

			var failResults = new List<ActionResult>();
			foreach (var personAbsenceGroup in personAbsenceGroups)
			{
				var cmd = new RemovePartPersonAbsenceCommand
				{
					Person = personAbsenceGroup.Person,
					ScheduleDate = scheduleDate,
					PersonAbsences = personAbsenceGroup.PersonAbsences,
					PeriodToRemove = periodToRemove,
					TrackedCommandInfo = trackInfo
				};

				_commandDispatcher.Execute(cmd);

				if (cmd.Errors == null) continue;

				appendErrorMessage(failResults, cmd.Errors);
			}
			return failResults;
		}

		private void appendErrorMessage(ICollection<ActionResult> failResults, ActionErrorMessage error)
		{
			var existingFailResult = failResults.SingleOrDefault(r => r.PersonId == error.PersonId);
			if (existingFailResult == null)
			{
				failResults.Add(new ActionResult
				{
					PersonId = error.PersonId,
					ErrorMessages = error.ErrorMessages.ToList()
				});
			}
			else
			{
				existingFailResult.ErrorMessages = existingFailResult.ErrorMessages.Concat(error.ErrorMessages).ToList();
			}
		}
	}
}