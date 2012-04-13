using Autofac;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;
using Module = Autofac.Module;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.IoC
{
	public class MyTimeAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			registerCommonTypes(builder);
			registerPortalTypes(builder);
			registerRequestContextTypes(builder);
			registerScheduleTypes(builder);
			registerTeamScheduleTypes(builder);
			registerStudentAvailabilityTypes(builder);
			registerPreferenceTypes(builder);
			registerAutoMapperTypes(builder);
			registerRequestsType(builder);
		}

		private void registerRequestsType(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>();
			builder.RegisterType<PersonRequestProvider>().As<IPersonRequestProvider>();
			builder.RegisterType<TextRequestPersister>().As<ITextRequestPersister>();
		}

		private void registerAutoMapperTypes(ContainerBuilder builder)
		{
			builder.Register(c => Mapper.Engine).As<IMappingEngine>();
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.AssignableTo<Profile>()
				.As<Profile>()
				.InstancePerDependency();
			builder.RegisterType<StudentAvailabilityDomainData>();
			builder.RegisterType<StudentAvailabilityDayFormMappingProfile.StudentAvailabilityDayFormToStudentAvailabilityDay>().SingleInstance();
			builder.RegisterType<PreferenceDayInputMappingProfile.PreferenceDayInputToPreferenceDay>().SingleInstance();
			builder.RegisterType<TextRequestFormMappingProfile.TextRequestFormToPersonRequest>().As<ITypeConverter<TextRequestForm, IPersonRequest>>().SingleInstance();
		}

		private void registerPreferenceTypes(ContainerBuilder builder)
		{
			builder.RegisterType<PreferenceViewModelFactory>().As<IPreferenceViewModelFactory>();
			builder.RegisterType<PreferenceProvider>().As<IPreferenceProvider>();
			builder.RegisterType<PreferenceOptionsProvider>().As<IPreferenceOptionsProvider>();
			builder.RegisterType<PreferencePersister>().As<IPreferencePersister>();
			builder.RegisterType<PreferenceFeedbackProvider>().As<IPreferenceFeedbackProvider>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionForDisplayCreator>().As<IEffectiveRestrictionForDisplayCreator>();
			builder.RegisterType<WorkTimeMinMaxCalculator>().As<IWorkTimeMinMaxCalculator>();
		}

		private static void registerStudentAvailabilityTypes(ContainerBuilder builder)
		{
			builder.RegisterType<StudentAvailabilityViewModelFactory>().As<IStudentAvailabilityViewModelFactory>();
			
			builder.RegisterType<StudentAvailabilityPersister>().As<IStudentAvailabilityPersister>();
		}

		private static void registerScheduleTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleViewModelFactory>().As<IScheduleViewModelFactory>();
			builder.RegisterType<HeaderViewModelFactory>().As<IHeaderViewModelFactory>();
			builder.RegisterType<PeriodViewModelFactory>().As<IPeriodViewModelFactory>();
			builder.RegisterType<PeriodSelectionViewModelFactory>().As<IPeriodSelectionViewModelFactory>();
			builder.RegisterType<ScenarioProvider>().As<IScenarioProvider>();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>();
		}

		private static void registerTeamScheduleTypes(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>();
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>();
			builder.RegisterType<TeamProvider>().As<ITeamProvider>();
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>();
			builder.RegisterType<PrincipalFactory>().As<IPrincipalFactory>();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>().As<ISessionSpecificCookieDataProviderSettings>();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>();
			builder.RegisterType<DefaultTeamCalculator>().As<IDefaultTeamCalculator>();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<PortalViewModelFactory>().As<IPortalViewModelFactory>();
			builder.RegisterType<DatePickerGlobalizationViewModelFactory>().As<IDatePickerGlobalizationViewModelFactory>();
			builder.Register(c =>
			                 	{
			                 		if (DefinedLicenseDataFactory.LicenseActivator == null)
			                 			throw new DataSourceException("Missing datasource (no *.hbm.xml file available)!");
			                 		return DefinedLicenseDataFactory.LicenseActivator;
			                 	})
				.As<ILicenseActivator>();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>();
			builder.RegisterType<DefaultScenarioScheduleProvider>()
				.As<IScheduleProvider>()
				.As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();
			builder.RegisterType<LinkProvider>().As<ILinkProvider>();
			builder.RegisterType<SchedulePersonProvider>().As<ISchedulePersonProvider>();
			builder.RegisterType<ScheduleColorProvider>().As<IScheduleColorProvider>();
			builder.RegisterType<HasDayOffUnderFullDayAbsence>().As<IHasDayOffUnderFullDayAbsence>();
			builder.RegisterType<PersonPeriodProvider>().As<IPersonPeriodProvider>();
		}
	}
}
