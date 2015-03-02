using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	[CLSCompliant(false)]
	public class TeamScheduleController : Controller
	{
		private readonly INow _now;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamScheduleViewModelReworkedMapper _teamScheduleViewModelReworked;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly IToggleManager _toggleManager;

		public TeamScheduleController(INow now, ITeamScheduleViewModelFactory teamScheduleViewModelFactory, IDefaultTeamProvider defaultTeamProvider, ITeamScheduleViewModelReworkedMapper teamScheduleViewModelReworked, ITimeFilterHelper timeFilterHelper, IToggleManager toggleManager)
		{
			_now = now;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_defaultTeamProvider = defaultTeamProvider;
			_teamScheduleViewModelReworked = teamScheduleViewModelReworked;
			_timeFilterHelper = timeFilterHelper;
			_toggleManager = toggleManager;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ViewResult Index(DateOnly? date, Guid? id)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			if (!id.HasValue)
			{
				var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);
				if (defaultTeam == null || !defaultTeam.Id.HasValue)
				{
					return View("NoTeamsPartial", model: date.Value.Date.ToString("yyyyMMdd"));
				}
				else
				{
					id = defaultTeam.Id;
				}
			}

			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_SortSchedule_32092))
			{
				return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel());
			}

			return View("TeamSchedulePartialOriginal", _teamScheduleViewModelFactory.CreateViewModel(date.Value, id.Value));
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult TeamScheduleCurrentDate()
		{			 
			var calendar = CultureInfo.CurrentCulture.Calendar;
			return Json(
				new
				{
					NowYear = calendar.GetYear(_now.LocalDateOnly()),
					NowMonth = calendar.GetMonth(_now.LocalDateOnly()),
					NowDay = calendar.GetDayOfMonth(_now.LocalDateOnly())
				},
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult TeamSchedule(DateOnly selectedDate, ScheduleFilter filter, Paging paging)
		{
			var allTeamIds = filter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new TeamScheduleViewModelData
			{
				ScheduleDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging,
				TimeFilter = _timeFilterHelper.GetFilter(selectedDate, filter.FilteredStartTimes, filter.FilteredEndTimes, filter.IsDayOff, filter.IsEmptyDay),
				SearchNameText = filter.SearchNameText,
				TimeSortOrder = filter.TimeSortOrder
			};
			return Json(_teamScheduleViewModelReworked.Map(data), JsonRequestBehavior.AllowGet);
		}
	}
}