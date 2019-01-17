using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture, MyTimeWebTest]
	public class SettingsControllerTest 
	{
		public FakeLoggedOnUser User;
		public SettingsController Target;
		public FindPersonInfoFake FindPerson;
		public IHashFunction Hash;
		public FakePersonalSettingDataRepository PersonalSettings;
		public CurrentHttpContext HttpContext;
		public CurrentTenantFake CurrentTenant;

		[Test]
		public void ShouldReturnView()
		{
			var res = Target.Index();
			res.ViewName.Should().Be.EqualTo("RegionalSettingsPartial");
		}

		[Test]
		public void ShouldReturnSettingsViewModel()
		{
			var res = Target.GetSettings();
			res.Data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnPasswordView()
		{
			var res = Target.Password();
			res.ViewName.Should().Be.EqualTo("PasswordPartial");
		}

		[Test]
		public void ShouldUpdateCulture()
		{
			Target.UpdateCulture(1034);
			User.CurrentUser().PermissionInformation.Culture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateCultureRepresentingNull()
		{
			Target.UpdateCulture(-1);
			User.CurrentUser().PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldUpdateUiCulture()
		{
			Target.UpdateUiCulture(1034);
			User.CurrentUser().PermissionInformation.UICulture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateUiCultureRepresentingNull()
		{
			Target.UpdateUiCulture(-1);
			User.CurrentUser().PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldChangePassword()
		{
			var personInfo = new PersonInfo(new Tenant("Test"), User.CurrentUser().Id.Value);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "test", "old", Hash);
			FindPerson.Add(personInfo);
			var result =
				Target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" })
					.Data as ChangePasswordResultInfo;
			Assert.IsTrue(result.IsSuccessful);
		}

		[Test]
		public void ShouldNotChangePasswordIfNewPasswordIsNullOrEmpty()
		{
			var currentContext = new FakeHttpContext("/ChangePassword");
			currentContext.SetRequest(new FakeHttpRequest("/ChangePassword", new Uri("http://localhost/Settings/ChangePassword"), new Uri("http://localhost/schedule/")));
			using(HttpContext.OnThisThreadUse(currentContext))
			{
				var personInfo = new PersonInfo(new Tenant("Test"), User.CurrentUser().Id.Value);
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "test", "old", Hash);
				FindPerson.Add(personInfo);
				var result =
					Target.ChangePassword(new ChangePasswordViewModel { NewPassword = null, OldPassword = "old" })
						.Data as ChangePasswordResultInfo;

				Assert.IsFalse(result.IsSuccessful);
			}
		}
		
		[Test]
		public void ShouldGetCalendarLinkStatus()
		{
			var currentContext = new FakeHttpContext("/calendarlink");
			currentContext.SetRequest(new FakeHttpRequest("/calendarlink", new Uri("http://localhost/Settings/CalendarLinkStatus"), new Uri("http://localhost/schedule/")));
			using (HttpContext.OnThisThreadUse(currentContext))
			{
				PersonalSettings.PersistSettingValue("CalendarLinkSettings", new CalendarLinkSettings { IsActive = true });
				var result = Target.CalendarLinkStatus().Data as CalendarLinkViewModel;
				result.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldSetCalendarLinkStatus()
		{
			PersonalSettings.PersistSettingValue("CalendarLinkSettings", new CalendarLinkSettings { IsActive = true });

			Target.SetCalendarLinkStatus(false);
			PersonalSettings.FindValueByKey("CalendarLinkSettings", new CalendarLinkSettings { IsActive = true }).IsActive.Should().Be.False();
		}

		[Test]
		public void ShouldHandleChangePasswordError()
		{
			var currentContext = new FakeHttpContext("/calendarlink");
			currentContext.SetRequest(new FakeHttpRequest("/calendarlink", new Uri("http://localhost/Settings/CalendarLinkStatus"), new Uri("http://localhost/schedule/")));
			using (HttpContext.OnThisThreadUse(currentContext))
			{
				var personInfo = new PersonInfo(new Tenant("Test"), User.CurrentUser().Id.Value);
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "test", "old", Hash);
				FindPerson.Add(personInfo);
				var result =
					Target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "wrong" })
						.Data as ChangePasswordResultInfo;
				result.IsSuccessful.Should().Be.False();
			}
		}

		[Test]
		public void ShouldHandleUndefinedOldPasswordError()
		{
			var currentContext = new FakeHttpContext("/calendarlink");
			currentContext.SetRequest(new FakeHttpRequest("/calendarlink", new Uri("http://localhost/Settings/CalendarLinkStatus"), new Uri("http://localhost/schedule/")));
			using (HttpContext.OnThisThreadUse(currentContext))
			{
				var personInfo = new PersonInfo(new Tenant("Test"), User.CurrentUser().Id.Value);
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "test", "old", Hash);
				FindPerson.Add(personInfo);
				var result =
					Target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new" })
						.Data as ChangePasswordResultInfo;
				result.IsSuccessful.Should().Be.False();
			}
		}

		[Test]
		public void ShouldGetMobileQRCodeUrl()
		{
			CurrentTenant.Current().SetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl, "http://qrcode");
			Target.MobileQRCodeUrl().Data.Should().Be.EqualTo("http://qrcode");
		}
	}
}