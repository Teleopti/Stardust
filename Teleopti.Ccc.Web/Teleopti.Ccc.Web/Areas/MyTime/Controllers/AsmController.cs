using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;

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
			return View(_asmModelFactory.CreateViewModel());
		}
	}
}