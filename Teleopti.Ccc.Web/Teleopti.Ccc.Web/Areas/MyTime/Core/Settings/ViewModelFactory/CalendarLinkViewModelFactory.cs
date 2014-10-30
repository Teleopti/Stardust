using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public class CalendarLinkViewModelFactory : ICalendarLinkViewModelFactory
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ICalendarLinkIdGenerator _calendarLinkIdGenerator;

		public CalendarLinkViewModelFactory(ICurrentHttpContext currentHttpContext, ICalendarLinkIdGenerator calendarLinkIdGenerator)
		{
			_currentHttpContext = currentHttpContext;
			_calendarLinkIdGenerator = calendarLinkIdGenerator;
		}

		public CalendarLinkViewModel CreateViewModel(CalendarLinkSettings calendarLinkSettings, string actionName)
		{
			var calendarLinkViewModel = new CalendarLinkViewModel { IsActive = calendarLinkSettings.IsActive };
			if (calendarLinkViewModel.IsActive)
			{
				var calendarId = _calendarLinkIdGenerator.Generate();
				var requestUrl = _currentHttpContext.Current().Request.Url.OriginalString;
				calendarLinkViewModel.Url =
					requestUrl.Substring(0, requestUrl.LastIndexOf("Settings/" + actionName, System.StringComparison.Ordinal)) +
					"Share?id=" + calendarId;
			}
			return calendarLinkViewModel;
		}
	}
}