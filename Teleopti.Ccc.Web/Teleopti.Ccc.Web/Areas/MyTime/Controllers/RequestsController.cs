using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsController : Controller
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly ITextRequestPersister _textRequestPersister;
		private readonly IAbsenceRequestPersister _absenceRequestPersister;
		private readonly IShiftTradeRequestPersister _shiftTradeRequestPersister;
		private readonly IRespondToShiftTrade _respondToShiftTrade;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IUserTimeZone _userTimeZone;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory, 
								ITextRequestPersister textRequestPersister, 
								IAbsenceRequestPersister absenceRequestPersister, 
								IShiftTradeRequestPersister shiftTradeRequestPersister,
								IRespondToShiftTrade respondToShiftTrade, 
								IPermissionProvider permissionProvider,
								IUserTimeZone timeZone)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_textRequestPersister = textRequestPersister;
			_absenceRequestPersister = absenceRequestPersister;
			_shiftTradeRequestPersister = shiftTradeRequestPersister;
			_respondToShiftTrade = respondToShiftTrade;
			_permissionProvider = permissionProvider;
			_userTimeZone = timeZone;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ViewResult Index()
		{
			return View("RequestsPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Requests(Paging paging)
		{
			return Json(_requestsViewModelFactory.CreatePagingViewModel(paging), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult RequestDetail(Guid id)
		{
			return Json(_requestsViewModelFactory.CreateRequestViewModel(id), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult PersonalAccountPermission()
		{
			bool personalAccountPermission = _permissionProvider.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);

			return Json(personalAccountPermission, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult FetchAbsenceAccount(Guid absenceId, DateOnly date)
		{
			return Json(_requestsViewModelFactory.GetAbsenceAccountViewModel(absenceId, date), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult TextRequest(TextRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_textRequestPersister.Persist(form));
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult ShiftTradeRequest(ShiftTradeRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_shiftTradeRequestPersister.Persist(form));
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult ApproveShiftTrade(ShiftTradeRequestReplyForm form)
		{
			var model = _respondToShiftTrade.OkByMe(form.ID, form.Message);
			model.Status = Resources.WaitingThreeDots;
			return Json(model);
		}


		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult DenyShiftTrade(ShiftTradeRequestReplyForm form)
		{
			return Json(_respondToShiftTrade.Deny(form.ID, form.Message));
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult AbsenceRequest(AbsenceRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			try
			{
				return Json(_absenceRequestPersister.Persist(form));
			}
			catch (InvalidOperationException e)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return e.ExceptionToJson(Resources.RequestCannotUpdateDelete);
			}
		}

		[UnitOfWorkAction]
		[HttpDelete]
		[ActionName("RequestDetail")]
		public JsonResult RequestDelete(Guid id)
		{
			_textRequestPersister.Delete(id);
			return Json("");
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestSchedule(DateOnly selectedDate, string teamId, Paging paging)
		{
			//var calendarDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, CultureInfo.CurrentCulture.Calendar);
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = selectedDate, TeamId = new Guid(teamId), Paging = paging };
			return Json(_requestsViewModelFactory.CreateShiftTradeScheduleViewModel(data), JsonRequestBehavior.AllowGet);
		}

		private IList<DateTimePeriod> convertStringToUtcTimes(DateOnly selectedDate, string timesString)
		{
			var startTimesx = string.IsNullOrEmpty(timesString) ? new string[] {} : timesString.Split(',');
			var periodsAsString = from t in startTimesx
										 let parts = t.Split('-')
										 let start = parts[0]
										 let end = parts[1]
										 select new
										 {
											 Start = start,
											 End = end
										 };
			var periods = from ps in periodsAsString
							  select new
							  {
								  Start = selectedDate.Date.Add(TimeSpan.Parse(ps.Start)),
								  End = selectedDate.Date.Add(TimeSpan.Parse(ps.End)),
							  };
			var periodsList = periods.ToList();
			if (!periodsList.Any())
				periodsList.Add(new
					{
						Start = selectedDate.Date.Add(TimeSpan.FromHours(0)),
						End = selectedDate.Date.Add(TimeSpan.FromHours(36)),
					});
			var periodsDateUtc = from p in periodsList
										let start = TimeZoneHelper.ConvertToUtc(p.Start, _userTimeZone.TimeZone())
										let end = TimeZoneHelper.ConvertToUtc(p.End, _userTimeZone.TimeZone())
										let period = new DateTimePeriod(start, end)
										select period;

			var utcTimes = periodsDateUtc.ToList();
			return utcTimes;
		}

		private TimeFilterInfo GetFilter(DateOnly selectedDate, string filterStartTimes, string filterEndTimes, bool isDayOff)
		{
			TimeFilterInfo filter;
			if (string.IsNullOrEmpty(filterStartTimes) && string.IsNullOrEmpty(filterEndTimes))
			{
				filter = null;
			}
			else
			{
				var startTimes = convertStringToUtcTimes(selectedDate, filterStartTimes);
				var endTimes = convertStringToUtcTimes(selectedDate, filterEndTimes);

				filter = new TimeFilterInfo();
				filter.StartTimeStarts = startTimes.Select(x => x.StartDateTime).ToArray();
				filter.StartTimeEnds = startTimes.Select(x => x.EndDateTime).ToArray();
				filter.EndTimeStarts = endTimes.Select(x => x.StartDateTime).ToArray();
				filter.EndTimeEnds = endTimes.Select(x => x.EndDateTime).ToArray();
				filter.IsDayOff = isDayOff;
			}
			return filter;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestScheduleByFilterTime(DateOnly selectedDate, string teamId, string filteredStartTimes, string filteredEndTimes, bool isDayOff, Paging paging)
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = selectedDate, TeamId = new Guid(teamId), Paging = paging, TimeFilter = GetFilter(selectedDate, filteredStartTimes, filteredEndTimes, isDayOff) };
			return Json(_requestsViewModelFactory.CreateShiftTradeScheduleViewModel(data), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestScheduleForAllTeams(DateOnly selectedDate, string teamIds, Paging paging)
		{
			var allTeamIds = teamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelDataForAllTeams { ShiftTradeDate = selectedDate,TeamIds = allTeamIds, Paging = paging };
			return Json(_requestsViewModelFactory.CreateShiftTradeScheduleViewModelForAllTeams(data), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestPeriod()
		{
			return Json(_requestsViewModelFactory.CreateShiftTradePeriodViewModel(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestSwapDetails(Guid id)
		{
			var viewmodel = _requestsViewModelFactory.CreateShiftTradeRequestSwapDetails(id);
			return Json(viewmodel, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult ResendShiftTrade(Guid id)
		{
			var model = _respondToShiftTrade.ResendReferred(id);
			model.Status = Resources.ProcessingDotDotDot;
			return Json(model);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestMyTeam(DateOnly selectedDate)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeMyTeamSimpleViewModel(selectedDate), JsonRequestBehavior.AllowGet);
		}
	}
}
