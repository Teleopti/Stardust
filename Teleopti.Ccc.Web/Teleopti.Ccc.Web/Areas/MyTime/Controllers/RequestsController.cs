using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
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
		private readonly ITimeFilterHelper _timeFilterHelper;
		private readonly IToggleManager _toggleManager;
		private IRequestsShiftTradeScheduleViewModelFactory _shiftTradeScheduleViewModelFactory;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory, 
								ITextRequestPersister textRequestPersister, 
								IAbsenceRequestPersister absenceRequestPersister, 
								IShiftTradeRequestPersister shiftTradeRequestPersister,
								IRespondToShiftTrade respondToShiftTrade, 
								IPermissionProvider permissionProvider,
								ITimeFilterHelper timeFilterHelper,
								IToggleManager toggleManager, 
								IRequestsShiftTradeScheduleViewModelFactory shiftTradeScheduleViewModelFactory)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_textRequestPersister = textRequestPersister;
			_absenceRequestPersister = absenceRequestPersister;
			_shiftTradeRequestPersister = shiftTradeRequestPersister;
			_respondToShiftTrade = respondToShiftTrade;
			_permissionProvider = permissionProvider;
			_timeFilterHelper = timeFilterHelper;
			_toggleManager = toggleManager;
			_shiftTradeScheduleViewModelFactory = shiftTradeScheduleViewModelFactory;
		}

		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index()
		{
			return View("RequestsPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult Requests(Paging paging)
		{
			return Json(_requestsViewModelFactory.CreatePagingViewModel(paging), JsonRequestBehavior.AllowGet);
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
			bool personalAccountPermission = _permissionProvider.HasApplicationFunctionPermission(
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
		public virtual JsonResult ApproveShiftTrade(ShiftTradeRequestReplyForm form)
		{
			var model = _respondToShiftTrade.OkByMe(form.ID, form.Message);
			model.Status = Resources.WaitingThreeDots;
			return Json(model);
		}
		
		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult DenyShiftTrade(ShiftTradeRequestReplyForm form)
		{
			return Json(_respondToShiftTrade.Deny(form.ID, form.Message));
		}

		[UnitOfWork]
		[HttpPostOrPut]
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
				return Json(_absenceRequestPersister.Persist(form));
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
		public virtual JsonResult RequestDelete(Guid id)
		{
			_textRequestPersister.Delete(id);
			return Json("");
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult ShiftTradeRequestSchedule(DateOnly selectedDate, ScheduleFilter filter, Paging paging)
		{
			var allTeamIds = filter.TeamIds.Split(',').Select(teamId => new Guid(teamId)).ToList();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = selectedDate,
				TeamIdList = allTeamIds,
				Paging = paging,
				TimeFilter = _timeFilterHelper.GetFilter(selectedDate, filter.FilteredStartTimes, filter.FilteredEndTimes, filter.IsDayOff,filter.IsEmptyDay),
				SearchNameText = filter.SearchNameText,
				TimeSortOrder = filter.TimeSortOrder
			};
			if (data.TimeFilter == null && data.TimeSortOrder == null &&
				_toggleManager.IsEnabled(Toggles.MyTimeWeb_ShiftTradePossibleTradedSchedulesNoReadModel_36211))
			{
				return Json(_shiftTradeScheduleViewModelFactory.CreateViewModel(data));
			}
			return Json(_requestsViewModelFactory.CreateShiftTradeScheduleViewModel(data));
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
			return Json(_requestsViewModelFactory.CreateShiftTradeMyTeamSimpleViewModel(selectedDate), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult GetShiftTradeRequestMiscSetting(Guid id)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradePeriodViewModel(id).MiscSetting, JsonRequestBehavior.AllowGet);
		}
	}
}
