using System;
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

		public RequestsShiftTradeBulletinBoardController(IRequestsShiftTradebulletinViewModelFactory requestsShiftTradebulletinViewModelFactory)
		{
			_requestsShiftTradebulletinViewModelFactory = requestsShiftTradebulletinViewModelFactory;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult BulletinSchedules(DateOnly selectedDate, string teamIds, Paging paging)
		{
			 var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			 var data = new ShiftTradeScheduleViewModelDataForAllTeams { ShiftTradeDate = selectedDate, TeamIds = allTeamIds, Paging = paging };
			 return Json(_requestsShiftTradebulletinViewModelFactory.CreateShiftTradeBulletinViewModel(data), JsonRequestBehavior.AllowGet);
		}

    }

}
