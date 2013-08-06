using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using MbCache.Core;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class SettingsControllerTest
	{
		private IMappingEngine mappingEngine;
		private ILoggedOnUser loggedOnUser;
		private IModifyPassword modifyPassword;

		[SetUp]
		public void Setup()
		{
			mappingEngine = MockRepository.GenerateStrictMock<IMappingEngine>();
			loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			modifyPassword = MockRepository.GenerateStrictMock<IModifyPassword>();
		}

		[Test]
		public void IndexShouldReturnView()
		{
			var settingsPermissionViewModelFactory = MockRepository.GenerateMock<ISettingsPermissionViewModelFactory>();
			using (
				var target = new SettingsController(mappingEngine, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), settingsPermissionViewModelFactory, null, null))
			{
				var res = target.Index();
				res.ViewName.Should().Be.EqualTo("RegionalSettingsPartial");
			}
		}

		[Test]
		public void CulturesShouldReturnViewModel()
		{
			using (
				var target = new SettingsController(mappingEngine, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				var viewModel = new SettingsViewModel();
				var person = new Person();
				loggedOnUser.Expect(obj => obj.CurrentUser()).Return(person);
				mappingEngine.Expect(obj => obj.Map<IPerson, SettingsViewModel>(person)).Return(viewModel);
				var res = target.Cultures();
				res.Data.Should().Be.SameInstanceAs(viewModel);
			}
		}

		[Test]
		public void PassWordShouldReturnCorrectView()
		{
			using (
				var target = new SettingsController(mappingEngine, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
				var target = new SettingsController(null, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
				var target = new SettingsController(null, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
				var target = new SettingsController(null, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
				var target = new SettingsController(null, loggedOnUser, null,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				target.UpdateUiCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldChangePassword()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new")).Return(new ChangePasswordResultInfo {IsSuccessful = true});
			using (
				var target = new SettingsController(null, loggedOnUser, modifyPassword,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				var result =
					target.ChangePassword(new ChangePasswordViewModel {NewPassword = "new", OldPassword = "old"}).Data as
					IChangePasswordResultInfo;
				Assert.IsTrue(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldHandleChangePasswordError()
		{
			var person = new Person();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();

			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new"))
			              .Return(new ChangePasswordResultInfo {IsSuccessful = false});
			using (
				var target = new SettingsController(null, loggedOnUser, modifyPassword,
				                                    new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				var context = new FakeHttpContext("/");
				context.SetResponse(response);
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				target.ModelState.AddModelError("Error", "Error");

				var result =
					target.ChangePassword(new ChangePasswordViewModel {NewPassword = "new", OldPassword = "old"}).Data as
					IChangePasswordResultInfo;
				Assert.IsFalse(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldGetCalendarLinkStatus()
		{
			var calendarLinkSettingsPersisterAndProvider =
				MockRepository.GenerateMock<ICalendarLinkSettingsPersisterAndProvider>();
			var settings = new CalendarLinkSettings
				{
					IsActive = true
				};
			calendarLinkSettingsPersisterAndProvider.Stub(x => x.Get()).Return(settings);

			var generator = MockRepository.GenerateMock<ICalendarLinkIdGenerator>();
			generator.Stub(x => x.Generate()).Return("calendarLinkId");

			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"),
			                                                           new Uri("http://localhost/"));
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/CalendarLinkStatus"));
			using (
				var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, null,
				                                                                                  null,
				                                                                                  null, null,
				                                                                                  calendarLinkSettingsPersisterAndProvider,
				                                                                                  generator))
			{
				var context = new FakeHttpContext("/");
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				context.SetRequest(request);

				var result = target.CalendarLinkStatus().Data as CalendarLinkViewModel;
				result.IsActive.Should().Be.EqualTo(settings.IsActive);
				result.Url.Should().Be.EqualTo("http://xxx.xxx.xxx.xxx/Mytime/Share?id=" + target.Url.Encode("calendarLinkId"));
			}
		}

		[Test]
		public void ShouldActivateCalendarLink()
		{
			var calendarLinkSettingsPersisterAndProvider =
				MockRepository.GenerateMock<ICalendarLinkSettingsPersisterAndProvider>();
			calendarLinkSettingsPersisterAndProvider.Stub(x => x.Persist(new CalendarLinkSettings {IsActive = true})).IgnoreArguments()
			                                        .Return(new CalendarLinkSettings
				                                        {
					                                        IsActive = true
				                                        });

			var generator = MockRepository.GenerateMock<ICalendarLinkIdGenerator>();
			generator.Stub(x => x.Generate()).Return("calendarLinkId");

			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/SetCalendarLinkStatus"));

			using (var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, null, null, null, null, calendarLinkSettingsPersisterAndProvider, generator))
			{
				var context = new FakeHttpContext("/");
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				context.SetRequest(request);

				var result = target.SetCalendarLinkStatus(true).Data as CalendarLinkViewModel;
				result.IsActive.Should().Be.True();
				result.Url.Should().Be.EqualTo("http://xxx.xxx.xxx.xxx/Mytime/Share?id=" + target.Url.Encode("calendarLinkId"));
			}
		}

		[Test]
		public void ShouldDeactivateCalendarLink()
		{
			var calendarLinkSettingsPersisterAndProvider =
				MockRepository.GenerateMock<ICalendarLinkSettingsPersisterAndProvider>();
			calendarLinkSettingsPersisterAndProvider.Stub(x => x.Persist(new CalendarLinkSettings { IsActive = true })).IgnoreArguments()
													.Return(new CalendarLinkSettings
													{
														IsActive = false
													});

			var generator = MockRepository.GenerateMock<ICalendarLinkIdGenerator>();
			generator.Stub(x => x.Generate()).Return("calendarLinkId");

			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/SetCalendarLinkStatus"));
			using (var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, null, null, null, null, calendarLinkSettingsPersisterAndProvider, generator))
			{
				var context = new FakeHttpContext("/");
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				context.SetRequest(request);

				var result = target.SetCalendarLinkStatus(false).Data as CalendarLinkViewModel;
				result.IsActive.Should().Be.False();
				result.Url.Should().Be.Null();
			}
		}
	}

	
}