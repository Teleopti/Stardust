﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
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
	public class TeamScheduleController : ApiController
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IAbsencePersister _absencePersister;
		private readonly ISettingsPersisterAndProvider<AgentsPerPageSetting> _agentsPerPagePersisterAndProvider;
		private readonly ISwapMainShiftForTwoPersonsCommandHandler _swapMainShiftForTwoPersonsHandler;
		private readonly IHandleCommand<RemovePersonAbsenceCommand> _removePersonAbsenceCommandHandler;
		private readonly IHandleCommand<RemovePartPersonAbsenceCommand> _removePartPersonAbsenceCommandHandler;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory, ILoggedOnUser loggonUser,
			IPrincipalAuthorization principalAuthorization, IAbsencePersister absencePersister,
			ISettingsPersisterAndProvider<AgentsPerPageSetting> agentsPerPagePersisterAndProvider,
			ISwapMainShiftForTwoPersonsCommandHandler swapMainShiftForTwoPersonsHandler,
			IHandleCommand<RemovePersonAbsenceCommand> removePersonAbsenceCommandHandler,
			IHandleCommand<RemovePartPersonAbsenceCommand> removePartPersonAbsenceCommandHandler)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_loggonUser = loggonUser;
			_principalAuthorization = principalAuthorization;
			_absencePersister = absencePersister;
			_agentsPerPagePersisterAndProvider = agentsPerPagePersisterAndProvider;
			_swapMainShiftForTwoPersonsHandler = swapMainShiftForTwoPersonsHandler;
			_removePersonAbsenceCommandHandler = removePersonAbsenceCommandHandler;
			_removePartPersonAbsenceCommandHandler = removePartPersonAbsenceCommandHandler;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GetPermissions")]
		public virtual JsonResult<PermissionsViewModel> GetPermissions()
		{
			var permissions = new PermissionsViewModel
			{
				IsAddFullDayAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				IsAddIntradayAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence),
				IsSwapShiftsAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SwapShifts),
				IsRemoveAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveAbsence),
				IsModifyScheduleAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule)
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

			if (string.IsNullOrEmpty(keyword))
			{
				var siteTerm = myTeam.Site.Description.Name.Contains(" ")
					? "\"" + myTeam.Site.Description.Name + "\""
					: myTeam.Site.Description.Name;
				var teamTerm = myTeam.Description.Name.Contains(" ")
					? "\"" + myTeam.Description.Name + "\""
					: myTeam.Description.Name;
				keyword = siteTerm + " " + teamTerm;
			}

			var criteriaDictionary = SearchTermParser.Parse(keyword);

			var result =
				_teamScheduleViewModelFactory.CreateViewModel(criteriaDictionary, currentDate, pageSize, currentPageIndex, isOnlyAbsences);
			result.Keyword = keyword;

			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GetSchedules")]
		public virtual GroupScheduleViewModel GetSchedulesForPeople([FromUri] Guid[] personIds, DateTime date)
		{
			return getSchedulesForPeople(personIds, date);
		}

		[HttpPost, UnitOfWork, AddFullDayAbsencePermission, Route("api/TeamSchedule/AddFullDayAbsence")]
		public virtual JsonResult<List<FailActionResult>> AddFullDayAbsence(FullDayAbsenceForm form)
		{
			setTrackedCommandInfo(form.TrackedCommandInfo);
			var failResults = new List<FailActionResult>();
			foreach (var personId in form.PersonIds)
			{
				var command = new AddFullDayAbsenceCommand
				{
					PersonId = personId,
					AbsenceId = form.AbsenceId,
					StartDate = form.StartDate,
					EndDate = form.EndDate,
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
			var failResults = new List<FailActionResult>();
			foreach (var personId in form.PersonIds)
			{
				var command = new AddIntradayAbsenceCommand
				{
					PersonId = personId,
					AbsenceId = form.AbsenceId,
					StartTime = form.StartTime,
					EndTime = form.EndTime,
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

			var personAbsenceIdsForRemove = new HashSet<Guid>(command.PersonAbsenceIds);

			// Get all person absence id for selected person
			var schedules = getSchedulesForPeople(command.PersonIds.ToArray(), command.ScheduleDate);
			foreach (var schedule in schedules.Schedules)
			{
				foreach (var projection in schedule.Projection.Where(projection => projection.ParentPersonAbsence != null))
				{
					personAbsenceIdsForRemove.Add(projection.ParentPersonAbsence.Value);
				}
			}

			if (command.RemoveEntireCrossDayAbsence)
			{
				foreach (var personAbsenceId in personAbsenceIdsForRemove)
				{
					_removePersonAbsenceCommandHandler.Handle(new RemovePersonAbsenceCommand
					{
						PersonAbsenceId = personAbsenceId,
						TrackedCommandInfo = command.TrackedCommandInfo
					});
				}
			}
			else
			{
				var scheduleDateInUtc = TimeZoneInfo.ConvertTimeToUtc(command.ScheduleDate,
					_loggonUser.CurrentUser().PermissionInformation.DefaultTimeZone());
				var periodToRemove = new DateTimePeriod(scheduleDateInUtc, scheduleDateInUtc.AddDays(1));
				foreach (var personAbsenceId in personAbsenceIdsForRemove)
				{
					_removePartPersonAbsenceCommandHandler.Handle(new RemovePartPersonAbsenceCommand
					{
						PersonAbsenceId = personAbsenceId,
						PeriodToRemove = periodToRemove,
						TrackedCommandInfo = command.TrackedCommandInfo
					});
				}
			}
			return Ok();
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

		private GroupScheduleViewModel getSchedulesForPeople(Guid[] personIds, DateTime date)
		{
			var scheduleDateOnly = new DateOnly(date);
			return _teamScheduleViewModelFactory.CreateViewModelForPeople(personIds, scheduleDateOnly);
		}
	}
}
