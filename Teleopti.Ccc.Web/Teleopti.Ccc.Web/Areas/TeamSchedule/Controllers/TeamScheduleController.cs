using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private readonly IAbsencePersister _absencePersister;
		private readonly ISettingsPersisterAndProvider<AgentsPerPageSetting> _agentsPerPagePersisterAndProvider;
		private readonly ISwapMainShiftForTwoPersonsCommandHandler _swapMainShiftForTwoPersonsHandler;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory,
			ILoggedOnUser loggonUser,
			IAuthorization authorization, IAbsencePersister absencePersister,
			ISettingsPersisterAndProvider<AgentsPerPageSetting> agentsPerPagePersisterAndProvider,
			ISwapMainShiftForTwoPersonsCommandHandler swapMainShiftForTwoPersonsHandler)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_loggonUser = loggonUser;

			_authorization = authorization;
			_absencePersister = absencePersister;
			_agentsPerPagePersisterAndProvider = agentsPerPagePersisterAndProvider;
			_swapMainShiftForTwoPersonsHandler = swapMainShiftForTwoPersonsHandler;
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
				HasAddingPersonalActivityPermission =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity),
				HasAddingOvertimeActivityPermission =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity),
				HasRemoveActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveActivity),
				HasMoveActivityPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity),
				HasMoveInvalidOverlappedActivityPermission =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity),
				HasEditShiftCategoryPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.EditShiftCategory),
				HasRemoveOvertimePermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveOvertime),
				HasMoveOvertimePermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveOvertime)
			};

			return Json(permissions);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/SearchSchedules")]
		public virtual JsonResult<GroupScheduleViewModel> SearchSchedules(SearchDaySchedulesFormData input)
		{
			var currentDate = input.Date;

			if (input.SelectedTeamIds != null && !input.SelectedTeamIds.Any())
				return
					Json(new GroupScheduleViewModel {Schedules = new List<GroupScheduleShiftViewModel>(), Total = 0, Keyword = ""});

			var criteriaDictionary = SearchTermParser.Parse(input.Keyword);

			var result =
				_teamScheduleViewModelFactory.CreateViewModel(input.SelectedTeamIds ?? new Guid[0], criteriaDictionary, currentDate,
					input.PageSize, input.CurrentPageIndex, input.IsOnlyAbsences);
			result.Keyword = input.Keyword;

			return Json(result);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/SearchWeekSchedules")]
		public virtual JsonResult<GroupWeekScheduleViewModel> SearchWeekSchedules(SearchWeekSchedulesFormData input)
		{
			var currentDate = new DateOnly(input.Date);
			if (input.SelectedTeamIds != null && !input.SelectedTeamIds.Any())
				return
					Json(new GroupWeekScheduleViewModel
					{
						PersonWeekSchedules = new List<PersonWeekScheduleViewModel>(),
						Total = 0,
						Keyword = ""
					});

			var criteriaDictionary = SearchTermParser.Parse(input.Keyword);

			var result =
				_teamScheduleViewModelFactory.CreateWeekScheduleViewModel(input.SelectedTeamIds, criteriaDictionary, currentDate,
					input.PageSize, input.CurrentPageIndex);
			result.Keyword = input.Keyword;

			return Json(result);
		}


		[UnitOfWork, HttpPost, Route("api/TeamSchedule/GetSchedules")]
		public virtual GroupScheduleViewModel GetSchedulesForPeople(GetSchedulesForPeopleFormData form)
		{
			return getSchedulesForPeople(form.PersonIds, form.Date);
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
				return BadRequest(Resources.EndTimeMustBeGreaterThanStartTime);
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

	}
}