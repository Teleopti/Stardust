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
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Core.RequestContext;
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
			using (var target = new SettingsController(mappingEngine, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, settingsPermissionViewModelFactory))
			{
				var res = target.Index();
				res.ViewName.Should().Be.EqualTo("RegionalSettingsPartial");
			}
		}

		[Test]
		public void CulturesShouldReturnViewModel()
		{
			using (var target = new SettingsController(mappingEngine, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(mappingEngine, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
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
			using (var target = new SettingsController(null, loggedOnUser, modifyPassword, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				var result=target.ChangePassword(new ChangePasswordViewModel {NewPassword = "new", OldPassword = "old"}).Data as IChangePasswordResultInfo;
				Assert.IsTrue(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldHandleChangePasswordError()
		{
			var person = new Person();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new")).Return(new ChangePasswordResultInfo { IsSuccessful = false });
			using (var target = new SettingsController(null, loggedOnUser, modifyPassword, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null), null, null, null))
			{
				var context = new FakeHttpContext("/");
				context.SetResponse(response);
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				target.ModelState.AddModelError("Error", "Error");

				var result = target.ChangePassword(new ChangePasswordViewModel { NewPassword = "new", OldPassword = "old" }).Data as IChangePasswordResultInfo;
				Assert.IsFalse(result.IsSuccessful);
			}
		}

		[Test]
		public void ShouldGetCalendarLinkStatus()
		{
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var settings = new CalendarLinkSettings();
			personalSettingDataRepository.Stub(x => x.FindValueByKey("CalendarLinkSettings", settings)).IgnoreArguments().Return(settings);
			using (
				var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, loggedOnUser, null, null,
				                                                                                  personalSettingDataRepository,
				                                                                                  null, null))
			{
				var result = target.CalendarLinkStatus().Data as CalendarLinkSettings;
				result.Should().Be.SameInstanceAs(settings);
			}
		}

		[Test]
		public void ShouldActivateCalendarLink()
		{
			var person = new Person();
			var id = Guid.NewGuid();
			person.SetId(id);
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/SetCalendarLinkStatus"));
			var settings = new CalendarLinkSettings();
			var currentDatasource = MockRepository.GenerateMock<ICurrentDataSource>();
			const string dataName = "TestRepsitory";

			currentDatasource.Stub(x => x.CurrentName()).Return(dataName);
			personalSettingDataRepository.Stub(x => x.FindValueByKey("CalendarLinkSettings", settings)).IgnoreArguments().Return(settings);

			using (var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, loggedOnUser, null, null, personalSettingDataRepository, currentDatasource, null))
			{
				var context = new FakeHttpContext("/");
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				context.SetRequest(request);

				var result = target.SetCalendarLinkStatus(true).Data as string;
				personalSettingDataRepository.AssertWasCalled(x => x.PersistSettingValue(settings));
				Assert.IsTrue(settings.IsActive);
				Assert.AreEqual(result, "http://xxx.xxx.xxx.xxx/Mytime/Share?id=" + target.Url.Encode(StringEncryption.Encrypt(dataName + "/" + id)));
			}
		}

		[Test]
		public void ShouldDeactivateCalendarLink()
		{
			var person = new Person();
			var id = Guid.NewGuid();
			person.SetId(id);
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			var personalSettingDataRepository = MockRepository.GenerateMock<IPersonalSettingDataRepository>();
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.Url).Return(new Uri("http://xxx.xxx.xxx.xxx/Mytime/Settings/SetCalendarLinkStatus"));
			var settings = new CalendarLinkSettings();
			var currentDatasource = MockRepository.GenerateMock<ICurrentDataSource>();
			const string dataName = "TestRepsitory";
			currentDatasource.Stub(x => x.CurrentName()).Return(dataName);
			personalSettingDataRepository.Stub(x => x.FindValueByKey("CalendarLinkSettings", settings)).IgnoreArguments().Return(settings);
			using (var target = new StubbingControllerBuilder().CreateController<SettingsController>(null, loggedOnUser, null, null, personalSettingDataRepository, currentDatasource, null))
			{
				var context = new FakeHttpContext("/");
				target.ControllerContext = new ControllerContext(context, new RouteData(), target);
				context.SetRequest(request);

				var result = target.SetCalendarLinkStatus(false).Data as string;
				personalSettingDataRepository.AssertWasCalled(x => x.PersistSettingValue(settings));
				Assert.IsFalse(settings.IsActive);
				Assert.AreEqual(result, string.Empty);
			}
		}
	}
}