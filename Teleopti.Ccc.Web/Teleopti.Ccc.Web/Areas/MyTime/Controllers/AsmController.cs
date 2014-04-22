using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)]
	public class AsmController : Controller
	{
		private readonly IAsmViewModelFactory _asmModelFactory;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public AsmController(IAsmViewModelFactory asmModelFactory, ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_asmModelFactory = asmModelFactory;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public ViewResult Index()
		{
			var layoutViewModel = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger);
			layoutViewModel.CultureSpecific.Rtl = false; //for now - asm is always displayed "western style" for now
			ViewBag.LayoutBase = layoutViewModel;
			return View();
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Today(DateTime asmZeroLocal)
		{
			var model = _asmModelFactory.CreateViewModel(asmZeroLocal);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult AlertTimeSetting()
		{
			var asmAlertTime = _globalSettingDataRepository.FindValueByKey("AsmAlertTime", new AsmAlertTime());

			Console.WriteLine("[Test]Get alert time setting: {0}", asmAlertTime.SecondsBeforeChange);

			return Json(asmAlertTime, JsonRequestBehavior.AllowGet);
		}
	}
}