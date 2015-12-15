using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
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
	public class TeamScheduleController : Controller
	{
		private readonly INow _now;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamScheduleViewModelReworkedFactory _teamScheduleViewModelReworkedFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly IToggleManager _toggleManager;
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
			_toggleManager = toggleManager;
			_loggedOnUser = loggedOnUser;
			_teamScheduleViewModelReworkedFactory = teamScheduleViewModelReworkedFactory;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ViewResult Index(DateOnly? date, Guid? id)
		{

			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_EnhanceTeamSchedule_32580))
			{
				return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel());
			}

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
					NowYear = calendar.GetYear(_now.LocalDateTime()),
					NowMonth = calendar.GetMonth(_now.LocalDateTime()),
					NowDay = calendar.GetDayOfMonth(_now.LocalDateTime()),
					DateTimeFormat = _loggedOnUser.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern
				},
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPost]
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
			TeamScheduleViewModelReworked result;
			result = _toggleManager.IsEnabled(Toggles.MyTimeWeb_TeamScheduleNoReadModel_36210)
				? _teamScheduleViewModelReworkedFactory.GetViewModelNoReadModel(data)
				: _teamScheduleViewModelReworkedFactory.GetViewModel(data);
			return Json(result);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult DefaultTeam(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);

			if (defaultTeam == null || !defaultTeam.Id.HasValue)
			{
				Response.StatusCode = (int) HttpStatusCode.BadRequest;
				return Json(new { Message = UserTexts.Resources.NoTeamsAvailable }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				return Json(new { DefaultTeam = defaultTeam.Id.Value }, JsonRequestBehavior.AllowGet);
			}					
		}

	}
}