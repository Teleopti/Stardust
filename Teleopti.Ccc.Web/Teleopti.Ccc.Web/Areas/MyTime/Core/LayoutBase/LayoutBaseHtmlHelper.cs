using System.Web.Mvc;
using Teleopti.Ccc.Domain.Util;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase
{
	public class LayoutBaseHtmlHelper
	{
		private const string LayoutBaseViewModelKey = "LayoutBase";
		private readonly HtmlHelper _htmlHelper;

		public LayoutBaseHtmlHelper(HtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper;
		}

		public MvcHtmlString FullDirAttribute()
		{
			var layoutBaseViewModel = (LayoutBaseViewModel)_htmlHelper.ViewData[LayoutBaseViewModelKey];
			return new MvcHtmlString(layoutBaseViewModel.CultureSpecific.Rtl ? " dir=\"rtl\"" : string.Empty);
		}

		public MvcHtmlString FullLangAttribute()
		{
			var layoutBaseViewModel = (LayoutBaseViewModel)_htmlHelper.ViewData[LayoutBaseViewModelKey];
			return new MvcHtmlString(string.Format(" lang=\"{0}\"", layoutBaseViewModel.CultureSpecific.LanguageCode));
		}

		public MvcHtmlString FullDirClass()
		{
			var layoutBaseViewModel = (LayoutBaseViewModel)_htmlHelper.ViewData[LayoutBaseViewModelKey];
			return new MvcHtmlString(layoutBaseViewModel.CultureSpecific.Rtl ? " class=\"rtl\"" : string.Empty);
		}
	}
}