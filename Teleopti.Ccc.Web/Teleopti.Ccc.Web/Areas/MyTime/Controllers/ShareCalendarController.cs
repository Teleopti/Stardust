using System;
using System.Security.Cryptography;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class ShareCalendarController : Controller
	{
		private readonly ICalendarLinkIdGenerator _calendarLinkIdGenerator;
		private readonly ICalendarLinkGenerator _calendarLinkGenerator;

		public ShareCalendarController(ICalendarLinkIdGenerator calendarLinkIdGenerator, ICalendarLinkGenerator calendarLinkGenerator)
		{
			_calendarLinkIdGenerator = calendarLinkIdGenerator;
			_calendarLinkGenerator = calendarLinkGenerator;
		}

		[HttpGet]
		[NoCacheFilterMvc]
		public ActionResult iCal(string id, string type = "text/calendar")
		{
			if (string.IsNullOrEmpty(id))
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("Invalid url", "text/plain");
			}
			try
			{
				var calendarLinkId = _calendarLinkIdGenerator.Parse(id);
				return Content(_calendarLinkGenerator.Generate(calendarLinkId), type);
			}
			catch (FormatException)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("Invalid url", "text/plain");
			}
			catch (CryptographicException)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("Invalid url", "text/plain");
			}
			catch (PermissionException)
			{
				return Content("No permission for calendar sharing", "text/plain");
			}
			catch (InvalidOperationException)
			{
				return Content("Calendar sharing inactive", "text/plain");
			}
		}
	}
}
