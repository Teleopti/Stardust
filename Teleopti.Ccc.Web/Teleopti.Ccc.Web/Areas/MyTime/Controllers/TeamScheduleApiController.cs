using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleApiController : ApiController
	{
		private readonly INow _now;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly ITeamScheduleViewModelFactoryToggle75989Off _teamScheduleViewModelFactoryToggle75989Off;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _userTimeZone;

		public TeamScheduleApiController(
			INow now,
			IDefaultTeamProvider defaultTeamProvider,
			ITimeFilterHelper timeFilterHelper,
			IUserCulture userCulture,
			IUserTimeZone userTimeZone,
			ITeamScheduleViewModelFactory teamScheduleViewModelFactory, ITeamScheduleViewModelFactoryToggle75989Off teamScheduleViewModelFactoryToggle75989Off)
		{
			_now = now;
			_defaultTeamProvider = defaultTeamProvider;
			_timeFilterHelper = timeFilterHelper;
			_userCulture = userCulture;
			_userTimeZone = userTimeZone;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_teamScheduleViewModelFactoryToggle75989Off = teamScheduleViewModelFactoryToggle75989Off;
		}

		[UnitOfWork, Route("api/TeamSchedule/TeamScheduleCurrentDate"), HttpGet]
		public virtual object TeamScheduleCurrentDate()
		{
			var calendar = CultureInfo.CurrentCulture.Calendar;

			var localNow = _now.CurrentLocalDateTime(_userTimeZone.TimeZone());
			return new
			{
				NowYear = calendar.GetYear(localNow),
				NowMonth = calendar.GetMonth(localNow),
				NowDay = calendar.GetDayOfMonth(localNow),
				DateTimeFormat = _userCulture.GetCulture().DateTimeFormat.ShortDatePattern
			};
		}

		[UnitOfWork, Route("api/TeamSchedule/TeamScheduleOld"), HttpPost]
		public virtual TeamScheduleViewModelToggle75989Off TeamScheduleOld(TeamScheduleRequest teamScheduleRequest)
		{
			var allTeamIds = teamScheduleRequest.ScheduleFilter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(teamScheduleRequest.SelectedDate),
				TeamIdList = allTeamIds,
				Paging = teamScheduleRequest.Paging,
				TimeFilter = _timeFilterHelper.GetFilter(new DateOnly(teamScheduleRequest.SelectedDate), teamScheduleRequest.ScheduleFilter.FilteredStartTimes, teamScheduleRequest.ScheduleFilter.FilteredEndTimes,
					teamScheduleRequest.ScheduleFilter.IsDayOff, teamScheduleRequest.ScheduleFilter.IsEmptyDay),
				SearchNameText = teamScheduleRequest.ScheduleFilter.SearchNameText,
				TimeSortOrder = teamScheduleRequest.ScheduleFilter.TimeSortOrder
			};

			return _teamScheduleViewModelFactoryToggle75989Off.GetTeamScheduleViewModel(data);
		}

		[UnitOfWork, Route("api/TeamSchedule/TeamSchedule"), HttpPost]
		public virtual TeamScheduleViewModel TeamSchedule(TeamScheduleRequest teamScheduleRequest)
		{
			var allTeamIds = teamScheduleRequest.ScheduleFilter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(teamScheduleRequest.SelectedDate),
				TeamIdList = allTeamIds,
				Paging = teamScheduleRequest.Paging,
				TimeFilter = _timeFilterHelper.GetTeamSchedulesFilter(new DateOnly(teamScheduleRequest.SelectedDate), teamScheduleRequest.ScheduleFilter),
				SearchNameText = teamScheduleRequest.ScheduleFilter.SearchNameText,
				TimeSortOrder = teamScheduleRequest.ScheduleFilter.TimeSortOrder
			};

			return _teamScheduleViewModelFactory.GetTeamScheduleViewModel(data);
		}

		[UnitOfWork, Route("api/TeamSchedule/DefaultTeam"), HttpGet]
		public virtual object DefaultTeam(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.CurrentLocalDate(_userTimeZone.TimeZone());
			var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);

			if (defaultTeam?.Id != null)
			{
				return new {
					DefaultTeam = defaultTeam.Id.Value,
					DefaultTeamName = defaultTeam.SiteAndTeam
				};
			}

			return new { Message = UserTexts.Resources.NoTeamsAvailable };
		}
	}
}