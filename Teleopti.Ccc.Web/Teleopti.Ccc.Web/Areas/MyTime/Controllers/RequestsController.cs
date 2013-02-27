﻿using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsController : Controller
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly ITextRequestPersister _textRequestPersister;
		private readonly IAbsenceRequestPersister _absenceRequestPersister;
		private readonly IRespondToShiftTrade _respondToShiftTrade;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory, 
								ITextRequestPersister textRequestPersister, 
								IAbsenceRequestPersister absenceRequestPersister, 
								IRespondToShiftTrade respondToShiftTrade)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_textRequestPersister = textRequestPersister;
			_absenceRequestPersister = absenceRequestPersister;
			_respondToShiftTrade = respondToShiftTrade;
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

		[HttpPostOrPut]
		public void ShiftTradeRequest(ShiftTradeRequestForm form)
		{
			var foo = "sdfsdfsdf";
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult ApproveShiftTrade(Guid id)
		{
			return Json(_respondToShiftTrade.OkByMe(id));
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult DenyShiftTrade(Guid id)
		{
			return Json(_respondToShiftTrade.Deny(id));
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
		public EmptyResult RequestDelete(Guid id)
		{
			_textRequestPersister.Delete(id);
			return new EmptyResult();
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestSchedule(DateTime selectedDate)
		{
			return Json(_requestsViewModelFactory.CreateShiftTradeScheduleViewModel(selectedDate), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult ShiftTradeRequestPeriod()
		{
			return Json(_requestsViewModelFactory.CreateShiftTradePeriodViewModel(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPost]
		public JsonResult ShiftTradeRequestSwapDetails(Guid id)
		{
			var viewmodel = _requestsViewModelFactory.CreateShiftTradeRequestSwapDetails(id);
			return Json(viewmodel, JsonRequestBehavior.AllowGet);
		}
	}
}
