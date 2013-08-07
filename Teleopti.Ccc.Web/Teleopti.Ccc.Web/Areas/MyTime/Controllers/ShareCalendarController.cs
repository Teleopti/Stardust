using System;
using System.Security.Cryptography;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;

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
		public ContentResult iCal(string id)
		{
			try
			{
				var calendarLinkId = _calendarLinkIdGenerator.Parse(id);
				return Content(_calendarLinkGenerator.Generate(calendarLinkId), "text/plain");
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
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("No permission for calendar sharing", "text/plain");
			}
			catch (InvalidOperationException)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("Calendar sharing inactive", "text/plain");
			}
		}
	}
}
