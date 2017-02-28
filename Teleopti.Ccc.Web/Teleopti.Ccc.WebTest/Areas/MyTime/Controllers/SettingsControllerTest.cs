﻿using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
		}

		[Test]
		public void IndexShouldReturnView()
		{
			var settingsPermissionViewModelFactory = MockRepository.GenerateMock<ISettingsPermissionViewModelFactory>();
			using (
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), settingsPermissionViewModelFactory, null, null, null, null, null))
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
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, settingsViewModelFactory, null, null, null, null))
			{
				var res = target.GetSettings();
				res.Data.Should().Be.SameInstanceAs(viewModel);
			}
		}

		[Test]
		public void PassWordShouldReturnCorrectView()
		{
			using (
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null))
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
				var target = new SettingsController(loggedOnUser,new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null))
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
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null))
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
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null))
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
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, null))
			{
				target.UpdateUiCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldChangePassword()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var changePassword = MockRepository.GenerateStub<IChangePersonPassword>();
			changePassword.Expect(x => x.Modify(person.Id.Value, "old", "new"));

			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (
				var target = new SettingsController(loggedOnUser, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, changePassword))
			{
				var result = target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" }).Data as ChangePasswordResultInfo;
				Assert.IsTrue(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldHandleChangePasswordError()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var response = MockRepository.GenerateStub<FakeHttpResponse>();

			var changePassword = MockRepository.GenerateStub<IChangePersonPassword>();
			changePassword.Expect(x => x.Modify(person.Id.Value, "old", "new"))
				.Throw(new HttpException());

			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (var target = new SettingsController(loggedOnUser, 
													new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null, null, null, changePassword))
			{
				var context = new FakeHttpContext("/");
				context.SetResponse(response);
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				target.ModelState.AddModelError("Error", "Error");

				var result =
					target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" }).Data as ChangePasswordResultInfo;
				Assert.IsFalse(result.IsSuccessful);
			}
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
			var target = new SettingsController(null, null, null, null, calendarLinkSettingsPersisterAndProvider, null,
																					calendarLinkViewModelFactory, null);
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
			var target = new SettingsController(null, null, null, null, calendarLinkSettingsPersisterAndProvider, null,
												calendarLinkViewModelFactory, null);

			var result = target.SetCalendarLinkStatus(false).Data as CalendarLinkViewModel;
			result.Should().Be.SameInstanceAs(calendarLinkViewModel);
		}

	}
}