using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class AsmController : Controller
	{
		private readonly IAsmViewModelFactory _asmModelFactory;

		public AsmController(IAsmViewModelFactory asmModelFactory)
		{
			_asmModelFactory = asmModelFactory;
		}

		public ViewResult Index()
		{
			ViewBag.LayoutBase = new LayoutBaseViewModelFactory(new CultureSpecificViewModelFactory(), new DatePickerGlobalizationViewModelFactory()).CreateLayoutBaseViewModel();
			return View(_asmModelFactory.CreateViewModel());
		}
	}
}