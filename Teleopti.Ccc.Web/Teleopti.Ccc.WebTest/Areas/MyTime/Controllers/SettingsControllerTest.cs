using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class SettingsControllerTest
	{
		private ILoggedOnUser loggedOnUser;
		private IModifyPassword modifyPassword;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			modifyPassword = MockRepository.GenerateStrictMock<IModifyPassword>();
		}

		[Test]
		public void IndexShouldReturnView()
		{
			var settingsPermissionViewModelFactory = MockRepository.GenerateMock<ISettingsPermissionViewModelFactory>();
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), settingsPermissionViewModelFactory, null, null, null, null, null, null))
			{
				var res = target.Index();
				res.ViewName.Should().Be.EqualTo("RegionalSettingsPartial");
			}
		}

		[Test]
		public void SettingsShouldReturnViewModel()
		{
			var viewModel = new SettingsViewModel();
			var settingsViewModelFactory = MockRepository.GenerateMock<ISettingsViewModelFactory>();
			settingsViewModelFactory.Stub(x => x.CreateViewModel()).Return(viewModel);
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, settingsViewModelFactory, null, null, null, null, null))
			{
				var res = target.GetSettings();
				res.Data.Should().Be.SameInstanceAs(viewModel);
			}
		}

		[Test]
		public void PassWordShouldReturnCorrectView()
		{
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, null))
			{
				var res = target.Password();
				res.ViewName.Should().Be.EqualTo("PasswordPartial");
			}
		}

		[Test]
		public void ShouldUpdateCulture()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, null))
			{
				target.UpdateCulture(1034);
			}
			person.PermissionInformation.Culture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateCultureRepresentingNull()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, null))
			{
				target.UpdateCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldUpdateUiCulture()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, null))
			{
				target.UpdateUiCulture(1034);
			}
			person.PermissionInformation.UICulture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateUiCultureRepresentingNull()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (
				var target = new SettingsController(loggedOnUser, null,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, null))
			{
				target.UpdateUiCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldChangePassword()
		{
			//TODO: tenant rewrite when old schema is gone!
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new")).Return(new ChangePasswordResultInfo { IsSuccessful = true });
			using (
				var target = new SettingsController(loggedOnUser, modifyPassword,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, MockRepository.GenerateStub<IToggleManager>()))
			{
				var result =
					target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" }).Data as
					IChangePasswordResultInfo;
				Assert.IsTrue(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldHandleChangePasswordError()
		{
			//TODO: tenant rewrite when old schema is gone!
			var person = new Person();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();

			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new"))
						  .Return(new ChangePasswordResultInfo { IsSuccessful = false });
			using (var target = new SettingsController(loggedOnUser, modifyPassword,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null, MockRepository.GenerateStub<IToggleManager>()))
			{
				var context = new FakeHttpContext("/");
				context.SetResponse(response);
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				target.ModelState.AddModelError("Error", "Error");

				var result =
					target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" }).Data as
					IChangePasswordResultInfo;
				Assert.IsFalse(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldPersistInTenantSchema_RemoveMeWhenOldPasswordTestsAreRewrittenAndOldSchemaIsGone()
		{
			var person = new Person();
			person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo {ApplicationLogOnName = RandomName.Make()};
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new")).Return(new ChangePasswordResultInfo { IsSuccessful = true });
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.MultiTenancy_LogonUseNewSchema_33049)).Return(true);
			var changePersonPassword = MockRepository.GenerateMock<IChangePersonPassword>();
			using (var target = new SettingsController(loggedOnUser, modifyPassword,
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, changePersonPassword, toggleManager))
			{
				target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" });
			}
			changePersonPassword.AssertWasCalled(x => x.Modify(person.ApplicationAuthenticationInfo.ApplicationLogOnName, "old", "new"));
		}

		[Test]
		public void ShouldGetCalendarLinkStatus()
		{
			var calendarLinkSettingsPersisterAndProvider =
				MockRepository.GenerateMock<ISettingsPersisterAndProvider<CalendarLinkSettings>>();
			var calendarLinkSettings = new CalendarLinkSettings
				{
					IsActive = true
				};
			calendarLinkSettingsPersisterAndProvider.Stub(x => x.Get()).Return(calendarLinkSettings);

			var calendarLinkViewModelFactory = MockRepository.GenerateMock<ICalendarLinkViewModelFactory>();
			var calendarLinkViewModel = new CalendarLinkViewModel();
			calendarLinkViewModelFactory.Stub(x => x.CreateViewModel(calendarLinkSettings, "CalendarLinkStatus"))
			                            .Return(calendarLinkViewModel);
			var target = new SettingsController(null, null, null, null, null, calendarLinkSettingsPersisterAndProvider, null,
																					calendarLinkViewModelFactory, null, null);
			var result = target.CalendarLinkStatus().Data as CalendarLinkViewModel;
			result.Should().Be.SameInstanceAs(calendarLinkViewModel);
		}

		[Test]
		public void ShouldSetCalendarLinkStatus()
		{
			var calendarLinkSettingsPersisterAndProvider =
				MockRepository.GenerateMock<ISettingsPersisterAndProvider<CalendarLinkSettings>>();
			var calendarLinkSettings = new CalendarLinkSettings
			{
				IsActive = true
			};
			calendarLinkSettingsPersisterAndProvider.Stub(x => x.Persist(new CalendarLinkSettings { IsActive = true })).IgnoreArguments()
													.Return(calendarLinkSettings);


			var calendarLinkViewModelFactory = MockRepository.GenerateMock<ICalendarLinkViewModelFactory>();
			var calendarLinkViewModel = new CalendarLinkViewModel();
			calendarLinkViewModelFactory.Stub(x => x.CreateViewModel(calendarLinkSettings, "SetCalendarLinkStatus"))
										.Return(calendarLinkViewModel);
			var target = new SettingsController(null, null, null, null, null, calendarLinkSettingsPersisterAndProvider, null,
												calendarLinkViewModelFactory, null, null);

			var result = target.SetCalendarLinkStatus(false).Data as CalendarLinkViewModel;
			result.Should().Be.SameInstanceAs(calendarLinkViewModel);
		}

	}
}