using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability
{
	public static class StudentAvilabilityHtmlExtension
	{
		public static MvcHtmlString DayContent(this HtmlHelper<StudentAvailabilityViewModel> helper, DayViewModelBase model)
		{
			var div = new TagBuilder("div");
			div.AddCssClass("day-content pdt10");

			if (model is ScheduledDayViewModel)
			{
				var scheduledDayInfoViewModel = model as ScheduledDayViewModel;

				var spanTitle = new TagBuilder("span");
				spanTitle.AddCssClass("fullwidth displayblock");
				spanTitle.SetInnerText(scheduledDayInfoViewModel.Title);

				var spanTimeSpan = new TagBuilder("span");
				spanTimeSpan.AddCssClass("fullwidth displayblock clearrightfloat");
				spanTimeSpan.SetInnerText(scheduledDayInfoViewModel.TimeSpan);

				var spanSummary = new TagBuilder("span");
				spanSummary.AddCssClass("fullwidth displayblock");
				spanSummary.SetInnerText(scheduledDayInfoViewModel.Summary);

				div.InnerHtml = string.Concat(spanTitle, spanTimeSpan, spanSummary);
			}
			if (model is AvailableDayViewModel)
			{
				var availableViewModel = model as AvailableDayViewModel;

				var emptySpan = new TagBuilder("span");
				emptySpan.AddCssClass("fullwidth displayblock");
				emptySpan.SetInnerText(string.Empty);

				var spanTimeSpan = new TagBuilder("span");
				spanTimeSpan.AddCssClass("fullwidth displayblock clearrightfloat");
				spanTimeSpan.SetInnerText(availableViewModel.AvailableTimeSpan);

				div.InnerHtml = string.Concat(emptySpan, spanTimeSpan);
			}
			var mvcHtmlString = new MvcHtmlString(div.ToString());
			return mvcHtmlString;
		}
	}
}