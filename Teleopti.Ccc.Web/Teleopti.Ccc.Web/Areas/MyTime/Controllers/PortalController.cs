﻿using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class PortalController : Controller
	{
		private readonly IPortalViewModelFactory _viewModelFactory;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;

		public PortalController(IPortalViewModelFactory viewModelFactory, ILayoutBaseViewModelFactory layoutBaseViewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
		}

		[UnitOfWork]
		[TenantUnitOfWork]
		public virtual ActionResult Index()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.MyTime);

			var portalViewModel = _viewModelFactory.CreatePortalViewModel();
			return View(portalViewModel);
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult GetBadges(DateOnly from, DateOnly to)
		{
			return  Json(_viewModelFactory.GetBadges(new DateOnlyPeriod(from, to)), JsonRequestBehavior.AllowGet);
		}
	}
}