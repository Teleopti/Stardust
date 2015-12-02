using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
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
		private ILoggedOnUser _loggonUser;
		private readonly ICommandDispatcher _commandDispatcher;

		public TeamScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory,
			ITeamScheduleViewModelFactory teamScheduleViewModelFactory, ILoggedOnUser loggonUser,
			IPrincipalAuthorization principalAuthorization, ICommandDispatcher commandDispatcher)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_loggonUser = loggonUser;
			_principalAuthorization = principalAuthorization;
			_commandDispatcher = commandDispatcher;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GetPermissions")]
		public virtual JsonResult<PermissionsViewModel> GetPermissions()
		{
			var permissions = new PermissionsViewModel()
			{
				IsAddFullDayAbsenceAvailable =
					_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)
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
		public virtual JsonResult<IEnumerable<GroupScheduleShiftViewModel>> GroupScheduleNoReadModel(Guid groupId,
			DateTime date)
		{
			var schedules =
				_teamScheduleViewModelFactory.CreateViewModel(groupId, date);

			return Json(schedules);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/SearchSchedules")]
		public virtual JsonResult<GroupScheduleViewModel> SearchSchedules(string keyword, DateTime date)
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
				_teamScheduleViewModelFactory.CreateViewModel(criteriaDictionary, currentDate);
			result.Keyword = keyword;

			return Json(result);
		}

		[HttpPost, UnitOfWork, AddFullDayAbsencePermission, Route("api/TeamSchedule/AddFullDayAbsence")]
		public virtual JsonResult<bool> AddFullDayAbsence(FullDayAbsenceForm form)
		{
			if (form.TrackedCommandInfo != null) form.TrackedCommandInfo.OperatedPersonId = _loggonUser.CurrentUser().Id.Value;

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
				_commandDispatcher.Execute(command);
			}

			return Json(true);
		}
	}
}
