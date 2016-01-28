using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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
		private readonly IRequestsShiftTradeBulletinViewModelFactory _requestsShiftTradeBulletinViewModelFactory;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly IToggleManager _toggleManager;

		public RequestsShiftTradeBulletinBoardController(
			IRequestsShiftTradeBulletinViewModelFactory requestsShiftTradeBulletinViewModelFactory,
			ITimeFilterHelper timeFilterHelper, IToggleManager toggleManager)
		{
			_requestsShiftTradeBulletinViewModelFactory = requestsShiftTradeBulletinViewModelFactory;
			_timeFilterHelper = timeFilterHelper;
			_toggleManager = toggleManager;
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
				Json(_toggleManager.IsEnabled(Toggles.MyTimeWeb_ShiftTradeBoardNoReadModel_36662)
					? _requestsShiftTradeBulletinViewModelFactory.CreateShiftTradeBulletinViewModelFromRawData(data)
					: _requestsShiftTradeBulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data));
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
