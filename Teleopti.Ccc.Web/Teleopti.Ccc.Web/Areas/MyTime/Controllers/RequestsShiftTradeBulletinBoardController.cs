﻿using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsShiftTradeBulletinBoardController : Controller
	{
		private IRequestsShiftTradebulletinViewModelFactory _requestsShiftTradebulletinViewModelFactory;
		private readonly IUserTimeZone _userTimeZone;

		public RequestsShiftTradeBulletinBoardController(IRequestsShiftTradebulletinViewModelFactory requestsShiftTradebulletinViewModelFactory, IUserTimeZone userTimeZone)
		{
			_requestsShiftTradebulletinViewModelFactory = requestsShiftTradebulletinViewModelFactory;
			_userTimeZone = userTimeZone;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult BulletinSchedules(DateOnly selectedDate, string teamIds, Paging paging)
		{
			 var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			 var data = new ShiftTradeScheduleViewModelDataForAllTeams { ShiftTradeDate = selectedDate, TeamIds = allTeamIds, Paging = paging };
			 return Json(_requestsShiftTradebulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data), JsonRequestBehavior.AllowGet);
		}		
		
		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult BulletinSchedulesWithTimeFilter(DateOnly selectedDate, string teamIds, string filteredStartTimes, string filteredEndTimes, bool isDayOff, Paging paging)
		{
			 var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			 var filterHelper = new FilterHelper(_userTimeZone);
			 var data = new ShiftTradeScheduleViewModelDataForAllTeams { ShiftTradeDate = selectedDate, TeamIds = allTeamIds, Paging = paging, TimeFilter = filterHelper.GetFilter(selectedDate, filteredStartTimes, filteredEndTimes, isDayOff) };
			 return Json(_requestsShiftTradebulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data), JsonRequestBehavior.AllowGet);
		}

    }

}
