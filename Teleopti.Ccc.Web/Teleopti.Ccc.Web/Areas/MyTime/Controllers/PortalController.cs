using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

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

		[UnitOfWorkAction]
		[TenantUnitOfWork]
		public virtual ActionResult Index()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.MyTime);

			var portalViewModel = _viewModelFactory.CreatePortalViewModel();
			return View(portalViewModel);
		}
	}
}