using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class CiscoWidgetController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IAsmViewModelFactory _asmModelFactory;
		public CiscoWidgetController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IAsmViewModelFactory asmModelFactory)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_asmModelFactory = asmModelFactory;
		}

		public ViewResult Index()
		{
			var layoutViewModel = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger);
			layoutViewModel.CultureSpecific.Rtl = false; //for now - asm is always displayed "western style" for now
			ViewBag.LayoutBase = layoutViewModel;

			ViewBag.HasAsmPermission = _asmModelFactory.HasAsmPermission();
			return View();
		}
	}
}