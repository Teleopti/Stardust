﻿using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)]
	public class AsmController : Controller
	{
		private readonly IAsmViewModelFactory _asmModelFactory;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ScheduleChangeMailboxPoller _scheduleChangePoller;

		public AsmController(IAsmViewModelFactory asmModelFactory, ILayoutBaseViewModelFactory layoutBaseViewModelFactory,
			IGlobalSettingDataRepository globalSettingDataRepository
			, ScheduleChangeMailboxPoller scheduleChangePoller
			)
		{
			_asmModelFactory = asmModelFactory;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_globalSettingDataRepository = globalSettingDataRepository;
			_scheduleChangePoller = scheduleChangePoller;
		}

		public ViewResult Index()
		{
			var layoutViewModel = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger);
			layoutViewModel.CultureSpecific.Rtl = false; //for now - asm is always displayed "western style" for now
			ViewBag.LayoutBase = layoutViewModel;

			ViewBag.HasAsmPermission = _asmModelFactory.HasAsmPermission();
			return View();
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult Today(DateTime asmZeroLocal)
		{
			var model = _asmModelFactory.CreateViewModel(asmZeroLocal);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult AlertTimeSetting()
		{
			var asmAlertTime = _globalSettingDataRepository.FindValueByKey("AsmAlertTime", new AsmAlertTime());
			return Json(asmAlertTime, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult NotificationsTimeToStaySetting()
		{
			var notificationsTimeToStay = _globalSettingDataRepository.FindValueByKey("NotificationDurationTime", new NotificationDurationTime());
			return Json(notificationsTimeToStay, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork, MessageBrokerUnitOfWork]
		[HttpGet]
		public virtual JsonResult CheckIfScheduleHasUpdates(Guid mailboxId, DateTime startDate, DateTime endDate)
		{
			var hasUpdates = _scheduleChangePoller.Check(mailboxId, new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)));
			return Json(new
			{
				HasUpdates = hasUpdates,
				StartDate = startDate.ToShortDateString(),
				EndDate = endDate.ToShortDateString()
			}, JsonRequestBehavior.AllowGet);
		}
		[UnitOfWork, MessageBrokerUnitOfWork]
		[HttpGet]
		public virtual JsonResult StartPolling()
		{
			return Json(_scheduleChangePoller.StartPolling(), JsonRequestBehavior.AllowGet);
		}
	}

}