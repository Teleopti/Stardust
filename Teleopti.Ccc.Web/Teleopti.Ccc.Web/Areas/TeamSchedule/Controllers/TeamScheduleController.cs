using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
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
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly ILoggedOnUser _loggonUser;
		private readonly IAbsencePersister _absencePersister;
		private readonly ISettingsPersisterAndProvider<AgentsPerPageSetting> _agentsPerPagePersisterAndProvider;
		private readonly ICommonNameDescriptionSetting _commonNameDescriptionSetting;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISwapAndModifyServiceNew _swapAndModifyServiceNew;
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;

		public TeamScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory,
			ITeamScheduleViewModelFactory teamScheduleViewModelFactory, ILoggedOnUser loggonUser,
			IPrincipalAuthorization principalAuthorization, IAbsencePersister absencePersister,
			ISettingsPersisterAndProvider<AgentsPerPageSetting> agentsPerPagePersisterAndProvider,
			ICommonNameDescriptionSetting commonNameDescriptionSetting,
			IPersonRepository personRepository,
			IScheduleRepository scheduleRepository,
			IScenarioRepository scenarioRepository,
			ISwapAndModifyServiceNew swapAndModifyServiceNew,
			IScheduleDictionaryPersister scheduleDictionaryPersister)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_loggonUser = loggonUser;
			_principalAuthorization = principalAuthorization;
			_absencePersister = absencePersister;
			_agentsPerPagePersisterAndProvider = agentsPerPagePersisterAndProvider;
			_commonNameDescriptionSetting = commonNameDescriptionSetting;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
			_swapAndModifyServiceNew = swapAndModifyServiceNew;
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GetPermissions")]
		public virtual JsonResult<PermissionsViewModel> GetPermissions()
		{
			var permissions = new PermissionsViewModel
			{
				IsAddFullDayAbsenceAvailable =
					_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				IsAddIntradayAbsenceAvailable =
					_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence),
				IsSwapShiftsAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SwapShifts)
			};

			return Json(permissions);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/Group")]
		public virtual JsonResult<PagingGroupScheduleShiftViewModel> GroupSchedule(Guid groupId, DateTime date,
			int pageSize, int currentPageIndex)
		{
			int totalPage;
			var schedules =
				_groupScheduleViewModelFactory.LoadSchedulesWithPaging(groupId, date, pageSize, currentPageIndex, out totalPage)
					.ToList();
			var result = new PagingGroupScheduleShiftViewModel
			{
				GroupSchedule = schedules,
				TotalPages = totalPage
			};
			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GroupNoReadModel")]
		public virtual JsonResult<PagingGroupScheduleShiftViewModel> GroupScheduleNoReadModel(Guid groupId,
			DateTime date, int pageSize, int currentPageIndex)
		{
			var result = _teamScheduleViewModelFactory.CreateViewModel(groupId, date, pageSize, currentPageIndex);

			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/SearchSchedules")]
		public virtual JsonResult<GroupScheduleViewModel> SearchSchedules(string keyword, DateTime date, int pageSize,
			int currentPageIndex)
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
				_teamScheduleViewModelFactory.CreateViewModel(criteriaDictionary, currentDate, pageSize, currentPageIndex);
			result.Keyword = keyword;

			return Json(result);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/GetScheduleForPeople")]
		public virtual JsonResult<GroupScheduleViewModel> GetSchedulesForPeople(GroupScheduleInput input)
		{
			var result = _teamScheduleViewModelFactory.CreateViewModel(input);
			return Json(result);
		}

		[HttpPost, UnitOfWork, AddFullDayAbsencePermission, Route("api/TeamSchedule/AddFullDayAbsence")]
		public virtual JsonResult<List<AddAbsenceFailResult>> AddFullDayAbsence(FullDayAbsenceForm form)
		{
			if (form.TrackedCommandInfo != null) form.TrackedCommandInfo.OperatedPersonId = _loggonUser.CurrentUser().Id.Value;

			var failResults = new List<AddAbsenceFailResult>();
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
			if (form.TrackedCommandInfo != null) form.TrackedCommandInfo.OperatedPersonId = _loggonUser.CurrentUser().Id.Value;

			if (!form.IsValid())
			{
				return BadRequest(Resources.EndTimeMustBeGreaterOrEqualToStartTime);
			}

			var failResults = new List<AddAbsenceFailResult>();
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
		public virtual IHttpActionResult SwapShifts(SwapShiftForm form)
		{
			if (form.TrackedCommandInfo != null)
			{
				form.TrackedCommandInfo.OperatedPersonId = _loggonUser.CurrentUser().Id.Value;
			}

			var peoples = _personRepository.FindPeople(new[] {form.PersonIdFrom, form.PersonIdTo}).ToArray();
			var scheduleDateOnly = new DateOnly(form.ScheduleDate);

			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			var period = new DateTimePeriod(form.ScheduleDate.ToUniversalTime(), form.ScheduleDate.AddDays(1).ToUniversalTime());

			var dictionary =
				_scheduleRepository.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), defaultScenario,
					new PersonProvider(_personRepository), new ScheduleDictionaryLoadOptions(true, true), peoples);

			var dates = new List<DateOnly> {scheduleDateOnly};
			var lockDates = new List<DateOnly>();
			var businessRules = NewBusinessRuleCollection.Minimum();
			var scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);

			var errorResponses =
				_swapAndModifyServiceNew.Swap(peoples[0], peoples[1], dates, lockDates, dictionary, businessRules, scheduleTagSetter)
					.Where(r => r.Error).ToArray();

			if (errorResponses.Length > 0)
			{
				return Json(errorResponses.Select(r => new AddAbsenceFailResult
				{
					PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(r.Person),
					Message = new List<string> {r.Message}
				}));
			}

			var failResults = new List<AddAbsenceFailResult>();
			var conflicts = _scheduleDictionaryPersister.Persist(dictionary).ToArray();
			if (conflicts.Length > 0)
			{
				foreach (var c in conflicts)
				{
					var peopleId = c.InvolvedId();
					var people = peoples.SingleOrDefault(p => p.Id == peopleId);
					if (people != null)
					{
						failResults.Add(new AddAbsenceFailResult
						{
							PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(people),
							Message = new List<string> {Resources.ScheduleHasBeenChanged}
						});
					}
				}
			}

			return Json(failResults);
		}
	}
}
