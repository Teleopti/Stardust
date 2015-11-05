using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class CiscoWidgetController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IAsmViewModelFactory _asmModelFactory;
		private readonly IUserCulture _userCulture;
		public CiscoWidgetController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IAsmViewModelFactory asmModelFactory, IUserCulture userCulture)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_asmModelFactory = asmModelFactory;
			_userCulture = userCulture;
		}

		public ViewResult Index()
		{
			var layoutViewModel = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger);
			layoutViewModel.CultureSpecific.Rtl = false; //for now - asm is always displayed "western style" for now
			ViewBag.LayoutBase = layoutViewModel;

			ViewBag.HasAsmPermission = _asmModelFactory.HasAsmPermission();

			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();

			return View();
		}
	}
}