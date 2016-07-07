﻿using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class CiscoWidgetController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IAsmViewModelFactory _asmModelFactory;
		private readonly IMyReportViewModelFactory _myReportViewModelFactory;
		private readonly IUserCulture _userCulture;
		public CiscoWidgetController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IAsmViewModelFactory asmModelFactory, IUserCulture userCulture, IMyReportViewModelFactory myReportViewModelFactory)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_asmModelFactory = asmModelFactory;
			_userCulture = userCulture;
			_myReportViewModelFactory = myReportViewModelFactory;
		}

		public ViewResult Index()
		{
			var layoutViewModel = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel(Resources.AgentScheduleMessenger);
			layoutViewModel.CultureSpecific.Rtl = false; //for now - asm is always displayed "western style" for now
			ViewBag.LayoutBase = layoutViewModel;

			ViewBag.HasAsmPermission = _asmModelFactory.HasAsmPermission();
			ViewBag.HasMyReportPermission = _myReportViewModelFactory.HasMyReportPermission();

			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();

			return View();
		}
	}
}