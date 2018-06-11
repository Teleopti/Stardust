using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class UserControllerTest
	{
		[Test]
		public void ShouldGetTheCurrentIdentityName()
		{
			var person = PersonFactory.CreatePerson();
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider());
			dynamic result = target.CurrentUser();
			Assert.AreEqual("Pelle", result.UserName);
		}

		[Test]
		public void ShouldGetTheCurrentLoggonUserTimezone()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider());
			dynamic result = target.CurrentUser();
			Assert.AreEqual(TimeZoneInfo.Local.DisplayName, result.DefaultTimeZoneName);
		}
		

		[Test]
		public void ShouldGetTheCurrentLoggonUserCulture()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var culture = CultureInfoFactory.CreateUsCulture();
			var uiCulture = CultureInfoFactory.CreateSwedishCulture();
			person.PermissionInformation.SetCulture(culture);
			person.PermissionInformation.SetUICulture(uiCulture);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider());
			dynamic result = target.CurrentUser();
			Assert.AreEqual(uiCulture.IetfLanguageTag, result.Language);
			Assert.AreEqual(culture.Name, result.DateFormatLocale);
		}
	}

	public class FakeIanaTimeZoneProvider : IIanaTimeZoneProvider
	{
		public string IanaToWindows(string ianaZoneId)
		{
			return ianaZoneId;
		}

		public string WindowsToIana(string windowsZoneId)
		{
			return windowsZoneId;
		}
	}
}