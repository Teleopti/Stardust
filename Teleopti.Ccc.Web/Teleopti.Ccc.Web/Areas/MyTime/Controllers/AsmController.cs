using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Ccc.Web.Core;
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
			return View(new AsmViewModel{Hours=new List<string>(), Layers = new List<AsmLayer>{}});
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Today(DateTime clientLocalAsmZero)
		{
			var model = _asmModelFactory.CreateViewModel();
			model.Layers = new List<AsmLayer>
			               	{
			               		new AsmLayer
			               			{
			               				Color = "red",
			               				EndTimeText = "17:00",
			               				LengthInMinutes = 120,
			               				Payload = "phone",
			               				StartTimeText = "15:00",
			               				StartJavascriptBaseDate =clientLocalAsmZero.AddDays(1).AddHours(15).SubtractJavascriptBaseDate().TotalMilliseconds
			               			}
			               	};
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}