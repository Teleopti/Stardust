using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsShiftTradeBulletinBoardController : Controller
	{
		private readonly IRequestsShiftTradeBulletinViewModelFactory _requestsShiftTradeBulletinViewModelFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;
		
		public RequestsShiftTradeBulletinBoardController(
			IRequestsShiftTradeBulletinViewModelFactory requestsShiftTradeBulletinViewModelFactory,
			ITimeFilterHelper timeFilterHelper)
		{
			_requestsShiftTradeBulletinViewModelFactory = requestsShiftTradeBulletinViewModelFactory;
			_timeFilterHelper = timeFilterHelper;
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult BulletinSchedules(DateOnly selectedDate, string teamIds, Paging paging)
		{
			var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging
			};
			return
				Json(_requestsShiftTradeBulletinViewModelFactory.CreateShiftTradeBulletinViewModelFromRawData(data));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult BulletinSchedulesWithTimeFilter(DateOnly selectedDate, ScheduleFilter filter, Paging paging)
		{
			var allTeamIds = filter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging,
				TimeFilter = _timeFilterHelper.GetFilter(selectedDate, filter.FilteredStartTimes, filter.FilteredEndTimes, filter.IsDayOff, filter.IsEmptyDay),
			};
			return Json(_requestsShiftTradeBulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data));
		}
	}
}
