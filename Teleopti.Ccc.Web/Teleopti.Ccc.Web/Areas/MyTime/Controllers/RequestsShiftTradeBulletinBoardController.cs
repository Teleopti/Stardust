using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsShiftTradeBulletinBoardController : Controller
	{
		private readonly IRequestsShiftTradebulletinViewModelFactory _requestsShiftTradebulletinViewModelFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;

		public RequestsShiftTradeBulletinBoardController(
			IRequestsShiftTradebulletinViewModelFactory requestsShiftTradebulletinViewModelFactory,
			ITimeFilterHelper timeFilterHelper)
		{
			_requestsShiftTradebulletinViewModelFactory = requestsShiftTradebulletinViewModelFactory;
			_timeFilterHelper = timeFilterHelper;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult BulletinSchedules(DateOnly selectedDate, string teamIds, Paging paging)
		{
			var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelDataForAllTeams
			{
				ShiftTradeDate = selectedDate,
				TeamIds = allTeamIds,
				Paging = paging
			};
			return Json(_requestsShiftTradebulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data),
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult BulletinSchedulesWithTimeFilter(DateOnly selectedDate, string teamIds, string filteredStartTimes,
			string filteredEndTimes, bool isDayOff, bool isEmptyDay, Paging paging)
		{
			var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelDataForAllTeams
			{
				ShiftTradeDate = selectedDate,
				TeamIds = allTeamIds,
				Paging = paging,
				TimeFilter = _timeFilterHelper.GetFilter(selectedDate, filteredStartTimes, filteredEndTimes, isDayOff, isEmptyDay)
			};
			return Json(_requestsShiftTradebulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data),
				JsonRequestBehavior.AllowGet);
		}
	}
}
