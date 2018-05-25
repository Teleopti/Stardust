using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleApiController : ApiController
	{
		private readonly INow _now;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleApiController(
			INow now,
			IDefaultTeamProvider defaultTeamProvider,
			ITimeFilterHelper timeFilterHelper,
			ILoggedOnUser loggedOnUser,
			ITeamScheduleViewModelFactory teamScheduleViewModelFactory)
		{
			_now = now;
			_defaultTeamProvider = defaultTeamProvider;
			_timeFilterHelper = timeFilterHelper;
			_loggedOnUser = loggedOnUser;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
		}

		[UnitOfWork, Route("api/TeamSchedule/TeamScheduleCurrentDate"), HttpGet]
		public virtual object TeamScheduleCurrentDate()
		{
			var calendar = CultureInfo.CurrentCulture.Calendar;

			var serverDateTimeDontUse = _now.ServerDateTime_DontUse();
			return new
			{
				NowYear = calendar.GetYear(serverDateTimeDontUse),
				NowMonth = calendar.GetMonth(serverDateTimeDontUse),
				NowDay = calendar.GetDayOfMonth(serverDateTimeDontUse),
				DateTimeFormat = _loggedOnUser.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern
			};
		}

		[UnitOfWork, Route("api/TeamSchedule/TeamSchedule"), HttpPost]
		public virtual TeamScheduleViewModel TeamSchedule(TeamScheduleRequest request)
		{
			var allTeamIds = request.ScheduleFilter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(request.SelectedDate),
				TeamIdList = allTeamIds,
				Paging = request.Paging,
				TimeFilter = _timeFilterHelper.GetFilter(new DateOnly(request.SelectedDate), request.ScheduleFilter.FilteredStartTimes, request.ScheduleFilter.FilteredEndTimes,
					request.ScheduleFilter.IsDayOff, request.ScheduleFilter.IsEmptyDay),
				SearchNameText = request.ScheduleFilter.SearchNameText,
				TimeSortOrder = request.ScheduleFilter.TimeSortOrder
			};

			return _teamScheduleViewModelFactory.GetViewModelNoReadModel(data);
		}

		[UnitOfWork, Route("api/TeamSchedule/DefaultTeam"), HttpGet]
		public virtual object DefaultTeam(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.ServerDate_DontUse();
			var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);

			if (defaultTeam?.Id != null)
			{
				return new { DefaultTeam = defaultTeam.Id.Value };
			}

			return new { Message = UserTexts.Resources.NoTeamsAvailable };
		}

		public class TeamScheduleRequest
		{
			public DateTime SelectedDate { get; set; }
			public ScheduleFilter ScheduleFilter { get; set; }
			public Paging Paging { get; set; }
		}
	}
}