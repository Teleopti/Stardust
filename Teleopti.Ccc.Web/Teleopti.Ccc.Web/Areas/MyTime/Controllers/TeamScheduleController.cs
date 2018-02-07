using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleController : Controller
	{
		private readonly INow _now;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamScheduleViewModelReworkedFactory _teamScheduleViewModelReworkedFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleController(
			INow now,
			ITeamScheduleViewModelFactory teamScheduleViewModelFactory,
			IDefaultTeamProvider defaultTeamProvider,
			ITimeFilterHelper timeFilterHelper,
			IToggleManager toggleManager,
			ILoggedOnUser loggedOnUser,
			ITeamScheduleViewModelReworkedFactory teamScheduleViewModelReworkedFactory)
		{
			_now = now;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_defaultTeamProvider = defaultTeamProvider;
			_timeFilterHelper = timeFilterHelper;
			_loggedOnUser = loggedOnUser;
			_teamScheduleViewModelReworkedFactory = teamScheduleViewModelReworkedFactory;
		}

		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index(DateOnly? date, Guid? id)
		{
			return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel());
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult TeamScheduleCurrentDate()
		{
			var calendar = CultureInfo.CurrentCulture.Calendar;

			var serverDateTimeDontUse = _now.ServerDateTime_DontUse();
			return Json(
				new
				{
					NowYear = calendar.GetYear(serverDateTimeDontUse),
					NowMonth = calendar.GetMonth(serverDateTimeDontUse),
					NowDay = calendar.GetDayOfMonth(serverDateTimeDontUse),
					DateTimeFormat = _loggedOnUser.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern
				},
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPost]
		public virtual JsonResult TeamSchedule(DateOnly selectedDate, ScheduleFilter filter, Paging paging)
		{
			var allTeamIds = filter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new TeamScheduleViewModelData
			{
				ScheduleDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging,
				TimeFilter = _timeFilterHelper.GetFilter(selectedDate, filter.FilteredStartTimes, filter.FilteredEndTimes,
					filter.IsDayOff, filter.IsEmptyDay),
				SearchNameText = filter.SearchNameText,
				TimeSortOrder = filter.TimeSortOrder
			};
			var result = _teamScheduleViewModelReworkedFactory.GetViewModelNoReadModel(data);
			return Json(result);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult DefaultTeam(DateOnly? date)
		{
			if (!date.HasValue)   
				date = _now.ServerDate_DontUse();
			var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);

			if (defaultTeam?.Id != null)
			{
				return Json(new { DefaultTeam = defaultTeam.Id.Value }, JsonRequestBehavior.AllowGet);
			}

			return Json(new { Message = UserTexts.Resources.NoTeamsAvailable }, JsonRequestBehavior.AllowGet);
		}
	}
}