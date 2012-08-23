using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class AsmController : Controller
	{
		private readonly IAsmViewModelFactory _asmModelFactory;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;

		public AsmController(IAsmViewModelFactory asmModelFactory, ILayoutBaseViewModelFactory layoutBaseViewModelFactory)
		{
			_asmModelFactory = asmModelFactory;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
		}

		public ViewResult Index()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View(_asmModelFactory.CreateViewModel());
		}
	}
}