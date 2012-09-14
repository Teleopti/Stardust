using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenAsm)]
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
			return View();
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Today(DateTime asmZero)
		{
			var model = _asmModelFactory.CreateViewModel(asmZero);
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}