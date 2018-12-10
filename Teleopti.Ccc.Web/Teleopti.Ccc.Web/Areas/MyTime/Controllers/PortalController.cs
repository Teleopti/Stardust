using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class PortalController : Controller
	{
		private readonly IPortalViewModelFactory _viewModelFactory;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IAgentBadgeWithinPeriodProvider _agentBadgeWithinPeriodProvider;

		public PortalController(IPortalViewModelFactory viewModelFactory, ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IAgentBadgeWithinPeriodProvider agentBadgeWithinPeriodProvider)
		{
			_viewModelFactory = viewModelFactory;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_agentBadgeWithinPeriodProvider = agentBadgeWithinPeriodProvider;
		}

		[HttpGet]
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
		public virtual JsonResult GetBadges(DateTime from, DateTime to)
		{
			return  Json(_agentBadgeWithinPeriodProvider.GetBadges(new DateOnlyPeriod(new DateOnly(from), new DateOnly(to))), JsonRequestBehavior.AllowGet);
		}
	}
}