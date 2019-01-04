using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsController : Controller
	{
		private readonly IAbsenceRequestDetailViewModelFactory _absenceRequestDetailViewModelFactory;
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly ITextRequestPersister _textRequestPersister;
		private readonly IAbsenceRequestPersister _absenceRequestPersister;
		private readonly IShiftTradeRequestPersister _shiftTradeRequestPersister;
		private readonly IRespondToShiftTrade _respondToShiftTrade;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly RequestsShiftTradeScheduleViewModelFactory _shiftTradeScheduleViewModelFactory;
		private readonly ICancelAbsenceRequestCommandProvider _cancelAbsenceRequestCommandProvider;
		private readonly RequestsViewModelMapper _viewModelMapper;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ILoggedOnUser _loggedOnUser;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory,
			ITextRequestPersister textRequestPersister,
			IAbsenceRequestPersister absenceRequestPersister,
			IShiftTradeRequestPersister shiftTradeRequestPersister,
			IRespondToShiftTrade respondToShiftTrade,
			IPermissionProvider permissionProvider,
			ITimeFilterHelper timeFilterHelper,
			RequestsShiftTradeScheduleViewModelFactory shiftTradeScheduleViewModelFactory,
			IAbsenceRequestDetailViewModelFactory absenceRequestDetailViewModelFactory,
			ICancelAbsenceRequestCommandProvider cancelAbsenceRequestCommandProvider,
			RequestsViewModelMapper viewModelMapper,
			IUserTimeZone userTimeZone,
			ILoggedOnUser loggedOnUser)
		{
			_absenceRequestDetailViewModelFactory = absenceRequestDetailViewModelFactory;
			_requestsViewModelFactory = requestsViewModelFactory;
			_textRequestPersister = textRequestPersister;
			_absenceRequestPersister = absenceRequestPersister;
			_shiftTradeRequestPersister = shiftTradeRequestPersister;
			_respondToShiftTrade = respondToShiftTrade;
			_permissionProvider = permissionProvider;
			_timeFilterHelper = timeFilterHelper;
			_shiftTradeScheduleViewModelFactory = shiftTradeScheduleViewModelFactory;
			_cancelAbsenceRequestCommandProvider = cancelAbsenceRequestCommandProvider;
			_viewModelMapper = viewModelMapper;
			_userTimeZone = userTimeZone;
			_loggedOnUser = loggedOnUser;
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index()
		{
			return View("RequestsPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult Requests(Paging paging, RequestListFilter filter)
		{
			return Json(_requestsViewModelFactory.CreatePagingViewModel(paging, filter), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult RequestDetail(Guid id)
		{
			return Json(_requestsViewModelFactory.CreateRequestViewModel(id), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult PersonalAccountPermission()
		{
			var personalAccountPermission = _permissionProvider.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);

			return Json(personalAccountPermission, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult FetchAbsenceAccount(Guid absenceId, DateOnly date)
		{
			return Json(_requestsViewModelFactory.GetAbsenceAccountViewModel(absenceId, date), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult TextRequest(TextRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_textRequestPersister.Persist(form));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult ShiftTradeRequest(ShiftTradeRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_shiftTradeRequestPersister.Persist(form));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult ApproveShiftTrade(ShiftTradeRequestReplyForm form)
		{
			var model = _respondToShiftTrade.OkByMe(form.ID, form.Message);
			model.Status = Resources.WaitingThreeDots;
			return Json(model);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult DenyShiftTrade(ShiftTradeRequestReplyForm form)
		{
			return Json(_respondToShiftTrade.Deny(form.ID, form.Message));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult AbsenceRequest(AbsenceRequestForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			try
			{
				var model = form.ToModel(_userTimeZone, _loggedOnUser);
				var result = Retry.Handle<DeadLockVictimException>()
					.WaitAndRetry(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4))
					.Execute(() => _absenceRequestPersister.Persist(model));
				var viewModel = _viewModelMapper.Map(result);
				return Json(viewModel);
			}
			catch (InvalidOperationException e)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return e.ExceptionToJson(Resources.RequestCannotUpdateDelete);
			}
		}

		[UnitOfWork]
		[HttpDelete]
		[ActionName("RequestDetail")]
		[HandleOptimisticLockException]
		public virtual JsonResult RequestDelete(Guid id)
		{
			_textRequestPersister.Delete(id);
			return Json("");
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult CancelRequest(Guid id)
		{
			var commandResult = _cancelAbsenceRequestCommandProvider.CancelAbsenceRequest(id);

			var result = new RequestCommandHandlingResult(
				commandResult.AffectedRequestId.HasValue ? new [] { commandResult.AffectedRequestId.Value } : null,
				commandResult.ErrorMessages);

			if (result.Success)
			{
				result.RequestViewModel = _requestsViewModelFactory.CreateRequestViewModel(id);
			}

			return Json(result);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual ActionResult ShiftTradeRequestSchedule(DateOnly selectedDate, ScheduleFilter filter, Paging paging)
		{
			if (selectedDate > new DateOnly(2079,6,6)) return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "The given date was out of range."); 
			var allTeamIds = new List<Guid>();
			if (!string.IsNullOrEmpty(filter.TeamIds))
			{
				var teamIdStrings = filter.TeamIds.Split(',');
				foreach (var teamIdString in teamIdStrings)
				{
					Guid teamId;
					if (!Guid.TryParse(teamIdString.Trim(), out teamId)) continue;

					allTeamIds.Add(teamId);
				}
			}

			var timeFilter = _timeFilterHelper.GetFilter(selectedDate, filter.FilteredStartTimes, filter.FilteredEndTimes,
				filter.IsDayOff, filter.IsEmptyDay);

			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging,
				TimeFilter = timeFilter,
				SearchNameText = filter.SearchNameText,
				TimeSortOrder = filter.TimeSortOrder
			};

			var loadScheduleWithoutReadModel = data.TimeFilter == null && data.TimeSortOrder == null;

			return Json(loadScheduleWithoutReadModel
				? _shiftTradeScheduleViewModelFactory.CreateViewModel(_loggedOnUser.CurrentUser(), data)
				: _requestsViewModelFactory.CreateShiftTradeScheduleViewModel(data));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual ActionResult ShiftTradeMultiDaysSchedule(ShiftTradeMultiSchedulesForm input)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeMultiSchedulesViewModel(input));
		}

		[UnitOfWork]
		[HttpGet]
		public virtual ActionResult GetWFCTolerance(Guid personToId)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeToleranceViewModel(personToId), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult ShiftTradeRequestPeriod()
		{
			return Json(_requestsViewModelFactory.CreateShiftTradePeriodViewModel(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult ShiftTradeRequestSwapDetails(Guid id)
		{
			var viewmodel = _requestsViewModelFactory.CreateShiftTradeRequestSwapDetails(id);
			return Json(viewmodel, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[HandleOptimisticLockException]
		public virtual JsonResult ResendShiftTrade(Guid id)
		{
			var model = _respondToShiftTrade.ResendReferred(id);
			model.Status = Resources.ProcessingDotDotDot;
			return Json(model);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult ShiftTradeRequestMyTeam(DateOnly selectedDate)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeMyTeamSimpleViewModel(selectedDate),
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult ShiftTradeRequestMySite(DateOnly selectedDate)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeMySiteIdViewModel(selectedDate),
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult GetShiftTradeRequestMiscSetting(Guid id)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradePeriodViewModel(id).MiscSetting, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult AbsenceRequestDetail(Guid id)
		{
			return Json(_absenceRequestDetailViewModelFactory.CreateAbsenceRequestDetailViewModel(id),
				JsonRequestBehavior.AllowGet);
		}
	}
}