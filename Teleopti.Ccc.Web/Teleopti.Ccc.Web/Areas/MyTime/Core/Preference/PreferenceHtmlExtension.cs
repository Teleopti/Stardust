using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference
{
	public static class PreferenceHtmlExtension
	{
		public static MvcHtmlString DayContent(this HtmlHelper<PreferenceViewModel> helper, DayViewModelBase model)
		{
			var div = new TagBuilder("div");
			div.AddCssClass("day-content pdt10");

			if (model is PreferenceDayViewModel)
			{
				var preferenceDayViewModel = model as PreferenceDayViewModel;

				var emptySpan = new TagBuilder("span");
				emptySpan.AddCssClass("fullwidth displayblock");
				emptySpan.SetInnerText(string.Empty);

				var spanTimeSpan = new TagBuilder("span");
				spanTimeSpan.AddCssClass("preference fullwidth displayblock clearrightfloat");
				spanTimeSpan.InnerHtml = preferenceDayViewModel.Preference;

				div.InnerHtml = string.Concat(emptySpan, spanTimeSpan);
			}
			var mvcHtmlString = new MvcHtmlString(div.ToString());
			return mvcHtmlString;
		}
	}


}