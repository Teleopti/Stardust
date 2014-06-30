using Autofac;
using AutoMapper;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.ServiceBus;
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
			registerScheduleTypes(builder);
			registerTeamScheduleTypes(builder);
			registerStudentAvailabilityTypes(builder);
			registerPreferenceTypes(builder);
			registerAutoMapperTypes(builder);
			registerRequestsType(builder);
			registerSettingsTypes(builder);
			registerAsmTypes(builder);
			registerMessageBrokerTypes(builder);
		}

		private static void registerMessageBrokerTypes(ContainerBuilder builder)
		{
			builder.RegisterType<UserDataFactory>().As<IUserDataFactory>();
		}

		private static void registerAsmTypes(ContainerBuilder builder)
		{
			builder.RegisterType<AsmViewModelFactory>().As<IAsmViewModelFactory>();
			builder.RegisterType<AsmViewModelMapper>().As<IAsmViewModelMapper>();
			builder.RegisterType<MessageViewModelFactory>().As<IMessageViewModelFactory>();
			builder.RegisterType<PushMessageDialoguePersister>().As<IPushMessageDialoguePersister>();
		}

		private static void registerSettingsTypes(ContainerBuilder builder)
		{
			builder.RegisterType<PersonPersister>().As<IPersonPersister>().SingleInstance();
			builder.RegisterType<SettingsPermissionViewModelFactory>().As<ISettingsPermissionViewModelFactory>();
			builder.RegisterType<CalendarLinkSettingsPersisterAndProvider>().As<ICalendarLinkSettingsPersisterAndProvider>().SingleInstance();
			builder.RegisterType<CalendarLinkIdGenerator>().As<ICalendarLinkIdGenerator>().SingleInstance();
			builder.RegisterType<CalendarLinkGenerator>().As<ICalendarLinkGenerator>().SingleInstance();
			builder.RegisterType<CalendarLinkViewModelFactory>().As<ICalendarLinkViewModelFactory>().SingleInstance();
            builder.RegisterType<CalendarTransformer>().As<ICalendarTransformer>().SingleInstance();
            builder.RegisterType<FindSharedCalendarScheduleDays>().As<IFindSharedCalendarScheduleDays>().SingleInstance();
            builder.RegisterType<CheckCalendarActiveCommand>().As<ICheckCalendarActiveCommand>().SingleInstance();
            builder.RegisterType<CheckCalendarPermissionCommand>().As<ICheckCalendarPermissionCommand>().SingleInstance();
            builder.RegisterType<PrincipalAuthorizationFactory>().As<IPrincipalAuthorizationFactory>().SingleInstance();
		}

		private static void registerRequestsType(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>();
			builder.RegisterType<PersonRequestProvider>().As<IPersonRequestProvider>();
			builder.RegisterType<TextRequestPersister>().As<ITextRequestPersister>();
			builder.RegisterType<ShiftTradeRequestPersister>().As<IShiftTradeRequestPersister>();
			builder.RegisterType<AbsenceRequestPersister>().As<IAbsenceRequestPersister>();
			builder.RegisterType<ShiftTradeRequestProvider>().As<IShiftTradeRequestProvider>();
			builder.RegisterType<RespondToShiftTrade>().As<IRespondToShiftTrade>();
			builder.RegisterType<MyTimeWebPersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			builder.RegisterType<ShiftTradePeriodViewModelMapper>().As<IShiftTradePeriodViewModelMapper>();
			builder.RegisterType<ShiftTradeRequestMapper>().As<IShiftTradeRequestMapper>();
			builder.RegisterType<PossibleShiftTradePersonsProvider>().As<IPossibleShiftTradePersonsProvider>();
			builder.RegisterType<CreateHourText>().As<ICreateHourText>();
			builder.RegisterType<ShiftTradeTimeLineHoursViewModelFactory>().As<IShiftTradeTimeLineHoursViewModelFactory>();
		}

		private void registerAutoMapperTypes(ContainerBuilder builder)
		{
			builder.Register(c => Mapper.Engine).As<IMappingEngine>();
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.AssignableTo<Profile>()
				.SingleInstance()
				.As<Profile>();
			builder.RegisterType<StudentAvailabilityDomainData>();
			builder.RegisterType<StudentAvailabilityDayFormMappingProfile.StudentAvailabilityDayFormToStudentAvailabilityDay>().SingleInstance();
			builder.RegisterType<PreferenceDayInputMappingProfile.PreferenceDayInputToPreferenceDay>().SingleInstance();
			builder.RegisterType<PreferenceTemplateInputMappingProfile.PreferenceTemplateInputToExtendedPreferenceTemplate>().SingleInstance();
			builder.RegisterType<TextRequestFormMappingProfile.TextRequestFormToPersonRequest>().As<ITypeConverter<TextRequestForm, IPersonRequest>>().SingleInstance();
			builder.RegisterType<AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest>().As<ITypeConverter<AbsenceRequestForm, IPersonRequest>>().SingleInstance();
		}

		private static void registerPreferenceTypes(ContainerBuilder builder)
		{
			builder.RegisterType<PreferenceController>().EnableClassInterceptors();
			builder.RegisterType<PreferenceFeedbackController>().EnableClassInterceptors();

			builder.RegisterType<PreferenceViewModelFactory>().As<IPreferenceViewModelFactory>();
			builder.RegisterType<PreferencePeriodFeedbackViewModelFactory>().As<IPreferencePeriodFeedbackViewModelFactory>();
			builder.RegisterType<PreferenceProvider>().As<IPreferenceProvider>();
			builder.RegisterType<PreferenceOptionsProvider>().As<IPreferenceOptionsProvider>();
			builder.RegisterType<PreferencePersister>().As<IPreferencePersister>();
			builder.RegisterType<MustHaveRestrictionSetter>().As<IMustHaveRestrictionSetter>();
			builder.RegisterType<PreferenceFeedbackProvider>().As<IPreferenceFeedbackProvider>().SingleInstance();
			builder.RegisterType<PreferencePeriodFeedbackProvider>().As<IPreferencePeriodFeedbackProvider>().SingleInstance();
			builder.RegisterType<WorkTimeMinMaxRestrictionCreator>().As<IWorkTimeMinMaxRestrictionCreator>();
			builder.RegisterType<EffectiveRestrictionForDisplayCreator>().As<IEffectiveRestrictionForDisplayCreator>();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>();
			builder.RegisterType<RestrictionCombiner>().As<IEffectiveRestrictionCombiner>();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>();
			builder.RegisterType<WorkTimeMinMaxCalculator>().As<IWorkTimeMinMaxCalculator>();
			builder.RegisterType<SchedulePeriodTargetDayOffCalculator>().As<ISchedulePeriodTargetDayOffCalculator>().SingleInstance();
			builder.RegisterType<PeriodScheduledAndRestrictionDaysOff>().As<IPeriodScheduledAndRestrictionDaysOff>().SingleInstance();
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().SingleInstance();
			builder.RegisterType<ExtendedPreferencePredicate>().As<IExtendedPreferencePredicate>().SingleInstance();
			builder.RegisterType<RestrictionChecker>().As<ICheckerRestriction>();
			builder.RegisterType<PreferenceTemplateProvider>().As<IPreferenceTemplateProvider>();
			builder.RegisterType<PreferenceTemplatePersister>().As<IPreferenceTemplatePersister>();
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
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>();
			builder.RegisterType<ExtractBudgetGroupPeriods>().As<IExtractBudgetGroupPeriods>();
			builder.RegisterType<OvertimeAvailabilityPersister>().As<IOvertimeAvailabilityPersister>();
		}

		private static void registerTeamScheduleTypes(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>();
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>();
			builder.RegisterType<TeamProvider>().As<ITeamProvider>();
			builder.RegisterType<DefaultTeamProvider>().As<IDefaultTeamProvider>();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<PortalViewModelFactory>().As<IPortalViewModelFactory>();
			builder.RegisterType<DatePickerGlobalizationViewModelFactory>().As<IDatePickerGlobalizationViewModelFactory>();
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
			builder.RegisterType<PersonPeriodProvider>().As<IPersonPeriodProvider>();
			builder.RegisterType<ServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<AllowanceProvider>().As<IAllowanceProvider>();
			builder.RegisterType<AbsenceTimeProvider>().As<IAbsenceTimeProvider>();
			builder.RegisterType<AbsenceRequestProbabilityProvider>().As<IAbsenceRequestProbabilityProvider>();
		}
	}
}
