using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class TimeZoneController : ApiController
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;

		public TimeZoneController(ILoggedOnUser loggedOnUser, IIanaTimeZoneProvider ianaTimeZoneProvider)
		{
			_loggedOnUser = loggedOnUser;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
		}

		[UnitOfWork, Route("api/Global/TimeZone"), HttpGet]
		public virtual dynamic Timezones()
		{
			var defaultTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			return
				new
				{
					DefaultTimezone = defaultTimeZone.Id,
					Timezones = TimeZoneInfo.GetSystemTimeZones().Select(x => new
					{
						x.Id,
						IanaId = _ianaTimeZoneProvider.WindowsToIana(x.Id),
						Name = x.DisplayName
					})
				};
		}
	}
}