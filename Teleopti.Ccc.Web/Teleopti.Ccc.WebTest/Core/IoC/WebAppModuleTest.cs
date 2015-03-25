using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AutoMapper;
using Autofac;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;
using Teleopti.Ccc.Web.Areas.Tenant;
using Teleopti.Ccc.Web.Areas.Toggle;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Hangfire;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ITeamScheduleViewModelFactory = Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory.ITeamScheduleViewModelFactory;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class WebAppModuleTest
	{
		[SetUp]
		public void Setup()
		{
			requestContainer = buildContainer();
		}

		private ILifetimeScope buildContainer(Toggles toggle, bool value)
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			return buildContainer(toggleManager);
		}

		private ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc(new IocArgs()));
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new WebAppModule(new IocConfiguration(new IocArgs(), toggleManager)));
			builder.RegisterInstance(MockRepository.GenerateMock<IApplicationData>()).As<IApplicationData>();
			builder.RegisterInstance(new FakeHttpContext("")).As<HttpContextBase>();
			return builder.Build();
		}

		private ILifetimeScope requestContainer;
		
		[Test]
		public void ControllersShouldBeRegisteredPerInstance()
		{
			var controllerNew = requestContainer.Resolve<AuthenticationController>();
			var controllerNew2 = requestContainer.Resolve<AuthenticationController>();

			controllerNew
				.Should().Not.Be.SameInstanceAs(controllerNew2);
		}

		[Test]
		public void ShouldRegisterToggleHandlerController()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile, string.Empty);
				var container = new ContainerConfiguration().Configure(tempFile, new HttpConfiguration());
				var containerAdder = new ContainerBuilder();
				containerAdder.Register(_ => MockRepository.GenerateMock<ILicenseActivator>());
				containerAdder.Update(container);

				container.Resolve<ToggleHandlerController>()
					.Should().Not.Be.Null();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void ShouldResolveBadgeLeaderBoardRpeortController()
		{
				requestContainer.Resolve<BadgeLeaderBoardReportController>()
					.Should().Not.Be.Null();
		}

		[Test]
		public void ControllersShouldBeenRegistered()
		{
			requestContainer.Resolve<AuthenticationController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAsmController()
		{
			requestContainer.Resolve<AsmController>()
				.Should().Not.Be.Null();
		}

		[Test, Ignore("Och d� fungerar ju inte denna heller")]
		public void ShouldRegisterTenantController()
		{
			requestContainer.Resolve<AuthenticateController>()
				.Should().Not.Be.Null();
		}

        [Test]
        public void ShouldRegisterCalendarShareController()
        {
            requestContainer.Resolve<ShareCalendarController>()
                .Should().Not.Be.Null();
        }

		[Test]
		public void ShouldRegisterMessageBrokerController()
		{
			requestContainer.Resolve<UserDataController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAppConfigSettings()
		{
			requestContainer.Resolve<ISettings>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAsmMapper()
		{
			requestContainer.Resolve<IAsmViewModelFactory>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterApplicationPath()
		{
			requestContainer.Resolve<IPhysicalApplicationPath>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveSitesController()
		{
			requestContainer.Resolve<SitesController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterAuthenticationService()
		{
			requestContainer.Resolve<IIdentityLogon>()
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
			requestContainer.Resolve<IUserCulture>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCultureSpecificViewModelDataFactory()
		{
			requestContainer.Resolve<ICultureSpecificViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCurrentBusinessUnitProvider()
		{
			requestContainer.Resolve<ICurrentBusinessUnit>()
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
			requestContainer.Resolve<ISessionPrincipalFactory>()
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
		public void ShouldRegisterAbsenceTypesProvider()
		{
			requestContainer.Resolve<IAbsenceTypesProvider>()
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
		    DefinedLicenseDataFactory.SetLicenseActivator("asdf",
		        new LicenseActivator(null, DateTime.MinValue, 0, 0, LicenseType.Agent,
		            new Percent(), null, null));
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
		public void ShouldResolveScheduleController()
		{
			requestContainer.Resolve<ScheduleController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveStudentAvailabilityController()
		{
			var result = requestContainer.Resolve<AvailabilityController>();
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
		public void ShouldResolvePersonInfoController()
		{
			requestContainer.Resolve<PersonInfoController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveTeamController()
		{
			requestContainer.Resolve<TeamController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveTeamScheduleViewModelFactory()
		{
			requestContainer.Resolve<ITeamScheduleViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveTeamViewModelFactory()
		{
			requestContainer.Resolve<ITeamViewModelFactory>()
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
			using (var lifetime = requestContainer.BeginLifetimeScope("AutofacWebRequest"))
			{
				lifetime.Resolve<HttpContextBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpRequestBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpResponseBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpServerUtilityBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpSessionStateBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpApplicationStateBase>().Should().Not.Be.Null();
				lifetime.Resolve<HttpCachePolicyBase>().Should().Not.Be.Null();
				lifetime.Resolve<UrlHelper>().Should().Not.Be.Null();
			}
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
		public void ShouldRegisterAbsenceRequestPersister()
		{
			requestContainer.Resolve<IAbsenceRequestPersister>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCacheRuleSetProjectionServiceResult()
		{
			var mbCacheFactory = requestContainer.Resolve<IMbCacheFactory>();
			mbCacheFactory.ImplementationTypeFor(typeof (IRuleSetProjectionService))
				.Should().Be.EqualTo<RuleSetProjectionService>();
		}

		
		[Test]
		public void ShouldRegisterServiceBusSender()
		{
			requestContainer.Resolve<IServiceBusSender>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMakeRegionalFromPerson()
		{
			var result = requestContainer.Resolve<IMakeRegionalFromPerson>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMakeOrganizationMembershipFromPerson()
		{
			var result = requestContainer.Resolve<IMakeOrganisationMembershipFromPerson>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveRetrievePersonNameForPerson()
		{
			var result = requestContainer.Resolve<IRetrievePersonNameForPerson>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCacheTeleoptiPrincipalInternalsFactory()
		{
			var mbCacheFactory = requestContainer.Resolve<IMbCacheFactory>();
			mbCacheFactory.ImplementationTypeFor(typeof(TeleoptiPrincipalInternalsFactory))
				.Should().Be.EqualTo<TeleoptiPrincipalInternalsFactory>();
		}

		[Test]
		public void ShouldResolvePreferenceFeedbackControllerWithDependenciesBecauseOfBugInEnableClassInterceptors()
		{
			requestContainer.Resolve<PreferenceFeedbackController>()
				.Should().Not.Be.Null();
			requestContainer.Resolve<IPreferencePeriodFeedbackViewModelFactory>()
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldResolveExtendedPreferencePredicate()
		{
			var result = requestContainer.Resolve<IExtendedPreferencePredicate>();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveAnywhereTeamScheduleHub()
		{
			requestContainer.Resolve<GroupScheduleHub>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveAnywherePersonScheduleHub()
		{
			requestContainer.Resolve<PersonScheduleHub>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotRegisterUnitOfWorkFactory()
		{
			//make sure IUnitOfWork isn't registered - it should NOT!
			requestContainer.IsRegistered<IUnitOfWorkFactory>()
				.Should().Be.False();
		}

		[Test]
		public void ShouldRegisterCommandDispatcher()
		{
			requestContainer.Resolve<ICommandDispatcher>()
			                .Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterCommandHandlers()
		{
			requestContainer.Resolve<IHandleCommand<AddFullDayAbsenceCommand>>()
			                .Should().Not.Be.Null();
			requestContainer.Resolve<IHandleCommand<RemovePersonAbsenceCommand>>()
							.Should().Not.Be.Null();
			requestContainer.Resolve<IHandleCommand<AddSeatPlanCommand>>()
							.Should().Not.Be.Null();

		}

		[Test]
		public void ShouldRegisterEventsMessageSender()
		{
			requestContainer.Resolve<IEnumerable<IMessageSender>>()
				.OfType<EventsMessageSender>()
				.Single()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveEventHandlers()
		{
			requestContainer.Resolve<IEnumerable<IHandleEvent<FullDayAbsenceAddedEvent>>>()
							.Should().Not.Be.Null();
			requestContainer.Resolve<IEnumerable<IHandleEvent<ScheduleChangedEvent>>>()
			                .Should().Not.Be.Null();
			requestContainer.Resolve<IEnumerable<IHandleEvent<ProjectionChangedEvent>>>()
							.Should().Not.Be.Null();
			requestContainer.Resolve<IEnumerable<IHandleEvent<ScheduleProjectionReadOnlyChanged>>>()
							.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveGroupController()
		{
			requestContainer.Resolve<GroupPageController>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMyReportViewModelFactory()
		{
			requestContainer.Resolve<IMyReportViewModelFactory>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMyReportDailyMetricsMapper()
		{
			requestContainer.Resolve<IDailyMetricsMapper>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveMyReportDailyMetricsForDayQuery()
		{
			requestContainer.Resolve<IDailyMetricsForDayQuery>()
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldResolveRequestsShiftTradeScheduleFilterController()
		{
			requestContainer.Resolve<RequestsShiftTradeScheduleFilterController>()
				 .Should().Not.Be.Null();
		}

		[Test]
		public void ShouldResolveHangfireServerStartupTaskIfToggleEnabled()
		{
			using (var container = buildContainer(Toggles.RTA_HangfireEventProcessing_31237, true))
			{
				container.Resolve<IEnumerable<IBootstrapperTask>>()
					.Select(x => x.GetType())
					.Should()
					.Contain(typeof (HangfireServerStartupTask));
			}
		}

		[Test]
		public void ShouldNotResolveHangfireServerStartupTaskIfToggleDisabled()
		{
			using (var container = buildContainer(Toggles.RTA_HangfireEventProcessing_31237, false))
			{
				container.Resolve<IEnumerable<IBootstrapperTask>>()
					.Select(x => x.GetType())
					.Should()
					.Not
					.Contain(typeof (HangfireServerStartupTask));
			}
		}
	}
}
