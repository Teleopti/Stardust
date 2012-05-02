using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Autofac;
using Autofac.Core;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.WebTest.Core.IoC
{

	[TestFixture]
	public class MvcModuleTest
	{
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			var applicationData = mocks.DynamicMock<IApplicationData>();

			var containerOrg = new ContainerConfiguration().Configure();

			var containerAdder = new ContainerBuilder();
			containerAdder.RegisterModule(new AuthenticationModule(applicationData));
			containerAdder.Register(c => mocks.DynamicMock<HttpContextBase>());
			containerAdder.Update(containerOrg);

			requestContainer = containerOrg.BeginLifetimeScope("httpRequest");
		}

		private ILifetimeScope requestContainer;
		private MockRepository mocks;

		[Test]
		public void ControllersShouldBeRegisteredPerInstance()
		{
			var controller = requestContainer.Resolve<AuthenticationController>();
			var controller2 = requestContainer.Resolve<AuthenticationController>();

			controller
				.Should().Not.Be.SameInstanceAs(controller2);
		}

		[Test]
		public void ControllersShouldBeenRegistered()
		{
			requestContainer.Resolve<AuthenticationController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAppConfigSettings()
		{
			requestContainer.Resolve<ISettings>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterApplicationPath()
		{
			requestContainer.Resolve<IPhysicalApplicationPath>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAuthenticationService()
		{
			requestContainer.Resolve<IAuthenticator>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAvailableWindowsDataSources()
		{
			requestContainer.Resolve<IAvailableWindowsDataSources>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterBusinessUnitProvider()
		{
			requestContainer.Resolve<IBusinessUnitProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCultureProvider()
		{
			requestContainer.Resolve<ICultureProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCultureSpecificViewModelDataFactory()
		{
			requestContainer.Resolve<ICultureSpecificViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCurrentPrincipalProvider()
		{
			requestContainer.Resolve<ICurrentPrincipalProvider>()
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldRegisterDataSourcesProvider()
		{
			requestContainer.Resolve<IDataSourcesProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterDatePickerGlobalizationViewModelFactory()
		{
			requestContainer.Resolve<IDatePickerGlobalizationViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterErrorMessageProvider()
		{
			requestContainer.Resolve<IErrorMessageProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterLayoutBaseViewModelDataFactory()
		{
			requestContainer.Resolve<ILayoutBaseViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterMappingEngine()
		{
			var result = requestContainer.Resolve<IMappingEngine>();

			result.Should().Be.SameInstanceAs(Mapper.Engine);
		}

		[Test]
		public void ShouldRegisterPrincipalProvider()
		{
			requestContainer.Resolve<IPrincipalFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterRequestContextInitializer()
		{
			requestContainer.Resolve<IRequestContextInitializer>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterSessionSpecificCookieDataProviderSettings()
		{
			requestContainer.Resolve<ISessionSpecificCookieDataProviderSettings>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterSessionSpecificDataProvider()
		{
			requestContainer.Resolve<ISessionSpecificDataProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterSignInViewModelFactory()
		{
			requestContainer.Resolve<IAuthenticationViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMappingProfiles()
		{
			var result = requestContainer.Resolve<IEnumerable<Profile>>();

			var profileTypes = from t in typeof (ContainerConfiguration).Assembly.GetTypes()
			                   let isProfile = typeof (Profile).IsAssignableFrom(t)
			                   where isProfile
			                   select t;
			result.Select(p => p.GetType()).Should().Have.SameValuesAs(profileTypes);
		}

		[Test]
		public void ShouldResolvePortalViewModelFactory()
		{
			DefinedLicenseDataFactory.LicenseActivator = new LicenseActivator(null, DateTime.MinValue, 0, 0, LicenseType.Agent,
			                                                                  new Percent(), null, null);
			requestContainer.Resolve<IPortalViewModelFactory>()
				.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldResolveMenuViewModelFactory()
		{
			requestContainer.Resolve<IMenuViewModelFactory>()
				.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldResolvePreferenceController()
		{
			requestContainer.Resolve<PreferenceController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolvePreferenceOptionsProvider()
		{
			requestContainer.Resolve<IPreferenceOptionsProvider>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveRequestsController()
		{
			requestContainer.Resolve<RequestsController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveRouteAreaRedirector()
		{
			var result = requestContainer.Resolve<IRedirector>();
			result.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldResolveScheduleController()
		{
			requestContainer.Resolve<ScheduleController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveStudentAvailabilityController()
		{
			var result = requestContainer.Resolve<StudentAvailabilityController>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveStudentAvailabilityDayFormMappingTypeConverter()
		{
			var result1 =
				requestContainer.Resolve
					<StudentAvailabilityDayFormMappingProfile.StudentAvailabilityDayFormToStudentAvailabilityDay>();
			var result2 =
				requestContainer.Resolve
					<StudentAvailabilityDayFormMappingProfile.StudentAvailabilityDayFormToStudentAvailabilityDay>();

			result1.Should().Not.Be.Null();
			result2.Should().Be.SameInstanceAs(result1);
		}

		[Test]
		public void ShouldResolveStudentAvailabilityMappingDomainData()
		{
			var result1 = requestContainer.Resolve<StudentAvailabilityDomainData>();
			var result2 = requestContainer.Resolve<StudentAvailabilityDomainData>();

			result1.Should().Not.Be.Null();
			result2.Should().Not.Be.Null();
			result2.Should().Not.Be.SameInstanceAs(result1);
		}

		[Test]
		public void ShouldResolveTeamScheduleController()
		{
			requestContainer.Resolve<TeamScheduleController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveTeamScheduleViewModelFactory()
		{
			requestContainer.Resolve<ITeamScheduleViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveVirtualSchedulePeriodProvider()
		{
			var result = requestContainer.Resolve<IVirtualSchedulePeriodProvider>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolvePreferenceFeedbackProvider()
		{
			var result1 = requestContainer.Resolve<IPreferenceFeedbackProvider>();
			result1.Should().Not.Be.Null();
			var result2 = requestContainer.Resolve<IPreferenceFeedbackProvider>();
			result2.Should().Be.SameInstanceAs(result1);
		}

		[Test]
		public void ShouldResolveWebTypes()
		{
			requestContainer.Resolve<UrlHelper>().Should().Not.Be.Null();

			//var request = new SimpleWorkerRequest("/dummy", @"c:\inetpub\wwwroot\dummy", "dummy.html", null, new StringWriter());
			//HttpContext.Current = new HttpContext(request);
			requestContainer.Resolve<HttpContextBase>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpRequestBase>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpResponseBase>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpServerUtilityBase>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpSessionStateBase>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpApplicationStateBase>().Should().Not.Be.Null();
			//requestContainer.Resolve<HttpBrowserCapabilitiesBase>().Should().Not.Be.Null();
			//requestContainer.Resolve<HttpFileCollectionBase>().Should().Not.Be.Null();
			//requestContainer.Resolve<System.Web.Routing.RequestContext>().Should().Not.Be.Null();
			requestContainer.Resolve<HttpCachePolicyBase>().Should().Not.Be.Null();
			//requestContainer.Resolve<VirtualPathProvider>().Should().Not.Be.Null();
			requestContainer.Resolve<UrlHelper>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldThrowDatasourceExceptionIfNoDatasourceFileIsAvailable()
		{
			DefinedLicenseDataFactory.LicenseActivator = null;
			var ok = false;
			try
			{
				requestContainer.Resolve<ILicenseActivator>();
			}
			catch (DependencyResolutionException dependencyResolutionException)
			{
				if (dependencyResolutionException.InnerException is DataSourceException)
					ok = true;
			}
			ok.Should().Be.True();
		}

		[Test]
		public void ShouldResolveEffectiveRestrictionForDisplayCreator()
		{
			var result = requestContainer.Resolve<IEffectiveRestrictionForDisplayCreator>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveWorkTimeMinMaxCalculator()
		{
			var result = requestContainer.Resolve<IWorkTimeMinMaxCalculator>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveRuleSetProjectionService()
		{
			var result = requestContainer.Resolve<IRuleSetProjectionService>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCacheRuleSetProjectionServiceResult()
		{
			var mbCacheFactory = requestContainer.Resolve<IMbCacheFactory>();
			mbCacheFactory.ImplementationTypeFor(typeof (IRuleSetProjectionService))
				.Should().Be.EqualTo<RuleSetProjectionService>();
		}
	}
}
