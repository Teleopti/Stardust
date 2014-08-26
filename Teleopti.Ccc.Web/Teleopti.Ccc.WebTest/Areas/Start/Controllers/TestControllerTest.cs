using System;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
    [TestFixture]
	public class TestControllerTest
	{
		[Test]
		public void CanSwitchINowImplementation()
		{
			var dateSet = new DateTime(2000, 1, 2, 3, 4, 5);

			var modifyNow = MockRepository.GenerateStrictMock<IMutateNow>();
			modifyNow.Expect(mock => mock.Mutate(dateSet));

			using (var target = new TestController(modifyNow, null, null, null, null, null, null, null,null, null, null, null, null))
			{
				target.SetCurrentTime(dateSet.Ticks);
			}
		}

		[Test]
		public void PlainStupid()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			var httpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
            httpContext.Stub(x => x.Current()).Return(new FakeHttpContext(""));
            var identityProviderProvider = new IdentityProviderProvider(MockRepository.GenerateMock<IConfigurationWrapper>());
			var physicalApplicationPath = MockRepository.GenerateMock<IPhysicalApplicationPath>();
			physicalApplicationPath.Stub(x => x.Get()).Return("");
			var settings = MockRepository.GenerateMock<ISettings>();
			settings.Stub(x => x.nhibConfPath()).Return("bin");
			var loadPasswordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			using (var target = new TestController(new MutableNow(), null, sessionSpecificDataProvider, null, null, null, httpContext, MockRepository.GenerateMock<IFormsAuthentication>(), null, identityProviderProvider, loadPasswordPolicyService, settings, physicalApplicationPath))
			{
                target.BeforeScenario(true, "Windows");
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();
			}
            sessionSpecificDataProvider.AssertWasCalled(x => x.ExpireTicket());
			loadPasswordPolicyService.AssertWasCalled(x=>x.ClearFile());
            identityProviderProvider.DefaultProvider().Should().Be.EqualTo("urn:Windows");
		}

		[Test]
		public void ShouldUsePasswordPolicy()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			var httpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			httpContext.Stub(x => x.Current()).Return(new FakeHttpContext(""));
			var identityProviderProvider = new IdentityProviderProvider(MockRepository.GenerateMock<IConfigurationWrapper>());
			var physicalApplicationPath = MockRepository.GenerateMock<IPhysicalApplicationPath>();
			physicalApplicationPath.Stub(x => x.Get()).Return("");
			var settings = MockRepository.GenerateMock<ISettings>();
			settings.Stub(x => x.nhibConfPath()).Return("bin");
			var loadPasswordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			loadPasswordPolicyService.Stub(x => x.Path).PropertyBehavior();
			using (var target = new TestController(new MutableNow(), null, sessionSpecificDataProvider, null, null, null, httpContext, MockRepository.GenerateMock<IFormsAuthentication>(), null, identityProviderProvider, loadPasswordPolicyService, settings, physicalApplicationPath))
			{
				target.BeforeScenario(true, "Windows", true);
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();
			}
			sessionSpecificDataProvider.AssertWasCalled(x => x.ExpireTicket());
			loadPasswordPolicyService.AssertWasCalled(x => x.ClearFile());
			identityProviderProvider.DefaultProvider().Should().Be.EqualTo("urn:Windows");
			loadPasswordPolicyService.Path.Should().Be.EqualTo(".");
		}

		[Test]
		public void ShouldNotUsePasswordPolicy()
		{
			var sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			var httpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			httpContext.Stub(x => x.Current()).Return(new FakeHttpContext(""));
			var identityProviderProvider = new IdentityProviderProvider(MockRepository.GenerateMock<IConfigurationWrapper>());
			var physicalApplicationPath = MockRepository.GenerateMock<IPhysicalApplicationPath>();
			physicalApplicationPath.Stub(x => x.Get()).Return("");
			var settings = MockRepository.GenerateMock<ISettings>();
			settings.Stub(x => x.nhibConfPath()).Return("bin");
			var loadPasswordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			loadPasswordPolicyService.Stub(x => x.Path).PropertyBehavior();
			using (var target = new TestController(new MutableNow(), null, sessionSpecificDataProvider, null, null, null, httpContext, MockRepository.GenerateMock<IFormsAuthentication>(), null, identityProviderProvider, loadPasswordPolicyService, settings, physicalApplicationPath))
			{
				target.BeforeScenario(true, "Windows");
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();
			}
			sessionSpecificDataProvider.AssertWasCalled(x => x.ExpireTicket());
			loadPasswordPolicyService.AssertWasCalled(x => x.ClearFile());
			identityProviderProvider.DefaultProvider().Should().Be.EqualTo("urn:Windows");
			loadPasswordPolicyService.Path.Should().Be.EqualTo("bin");
		}

		[Test]
		public void EvenMoreStupider()
		{
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			authenticator.Stub(x => x.AuthenticateApplicationUser(null, null, null)).IgnoreArguments().Return(new AuthenticateResult { Person = person });
			var logon = MockRepository.GenerateMock<IWebLogOn>();
			var businessUnit = new BusinessUnit("businessUnit");
			businessUnit.SetId(Guid.NewGuid());
			var businessUnitProvider = MockRepository.GenerateMock<IBusinessUnitProvider>();
			businessUnitProvider.Stub(x => x.RetrieveBusinessUnitsForPerson(null, null)).IgnoreArguments().Return(new[] { businessUnit });
			var httpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			httpContext.Stub(x => x.Current()).Return(new FakeHttpContext(""));
			using (var target = new TestController(null, null, null, authenticator, logon, businessUnitProvider, httpContext, null, null, null, null, null, null))
			{
				target.Logon(null, businessUnit.Name, null, null);
			}
		}

		[Test]
		public void CheckFeature_WhenToggleIsDisabled_ShouldReturnFalse()
		{
			const string toggle = "TestToggle";
			var toggleStub = MockRepository.GenerateMock<IToggleManager>();


			toggleStub.Expect(t => t.IsEnabled(Toggles.TestToggle)).Return(false);

			using (var target = new TestController(null, null, null, null, null, null, null, null, toggleStub, null, null, null, null))
			{
				var model = target.CheckFeature(toggle).Model as TestMessageViewModel;
				Assert.That(model.Title.Contains(toggle));
				Assert.That(model.Message.Contains(false.ToString()));
			}
		}

		[Test]
		public void CheckFeature_WhenToggleIsEnabled_ShouldReturnTrue()
		{
			const string toggle = "TestToggle";
			var toggleStub = MockRepository.GenerateMock<IToggleManager>();


			toggleStub.Expect(t => t.IsEnabled(Toggles.TestToggle)).Return(true);

			using (var target = new TestController(null, null, null, null, null, null, null, null, toggleStub, null, null, null, null))
			{
				var model = target.CheckFeature(toggle).Model as TestMessageViewModel;
				Assert.That(model.Message.Contains(true.ToString()));

			}
		}

		[Test]
		public void CheckFeature_WhenToggleDoesNotExist_ShouldReturnFalse()
		{
			const string toggle = "NonExistingFeature";
			var toggleStub = MockRepository.GenerateMock<IToggleManager>();

			using (var target = new TestController(null, null, null, null, null, null, null, null, toggleStub, null, null, null, null))
			{
				var model = target.CheckFeature(toggle).Model as TestMessageViewModel;
				Assert.That(model.Message.Contains(false.ToString()));

			}
		}

		
	}
}