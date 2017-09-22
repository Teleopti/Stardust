using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class TimeZoneController : ApiController
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public TimeZoneController(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, Route("api/Global/TimeZone"), HttpGet]
		public virtual dynamic Timezones()
		{
			var defaultTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			return
				new
				{
					DefaultTimezone = defaultTimeZone.Id,
					Timezones = TimeZoneInfo.GetSystemTimeZones().Select(x => new { x.Id, Name = x.DisplayName })
				};
		}
	}
}