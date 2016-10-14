using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class TimeZoneControllerTest
	{
		[Test]
		public void ShouldGetDefaultTimezone()
		{
			var loggedOnUser = new FakeLoggedOnUser(PersonFactory.CreatePerson("test1"));
			var target = new TimeZoneController(loggedOnUser);
			var result = target.Timezones();
			Assert.AreEqual(loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().Id, result.DefaultTimezone);
		}

		[Test]
		public void ShouldGetTimezones()
		{
			var loggedOnUser = new FakeLoggedOnUser(PersonFactory.CreatePerson("test1"));
			var target = new TimeZoneController(loggedOnUser);
			var timezones = TimeZoneInfo.GetSystemTimeZones();
			var result = target.Timezones();
			Assert.AreEqual(timezones.Count, ((IEnumerable<dynamic>)result.Timezones).Count());
		}
	}
}