using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.CommandProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Core;
using Module = Autofac.Module;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.IoC
{
	public class MyTimeAreaModule : Module
	{
		private readonly IocConfiguration _config;

		public MyTimeAreaModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			registerCommonTypes(builder);
			registerPortalTypes(builder);
			registerScheduleTypes(builder);
			registerTeamScheduleTypes(builder);
			registerStudentAvailabilityTypes(builder);
			registerPreferenceTypes(builder);
			registerMapperTypes(builder);
			registerRequestsType(builder);
			registerSettingsTypes(builder);
			registerAsmTypes(builder);
			registerMessageBrokerTypes(builder);
			registerMyReportTypes(builder);
			registerBadgeLeaderBoardReportTypes(builder);
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
			builder.RegisterType<ScheduleChangeMessagePoller>().SingleInstance();
		}

		private void registerSettingsTypes(ContainerBuilder builder)
		{
			builder.RegisterType<PersonPersister>().As<IPersonPersister>().SingleInstance();
			builder.RegisterType<SettingsPermissionViewModelFactory>().As<ISettingsPermissionViewModelFactory>();
			builder.RegisterType<SettingsViewModelFactory>().As<ISettingsViewModelFactory>().SingleInstance();
			builder.RegisterType<CalendarLinkSettingsPersisterAndProvider>()
				.As<ISettingsPersisterAndProvider<CalendarLinkSettings>>()
				.SingleInstance();

			builder.RegisterType<NameFormatSettingsPersisterAndProvider>()
				.As<ISettingsPersisterAndProvider<NameFormatSettings>>()
				.SingleInstance();
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
			builder.RegisterType<AbsenceAccountProvider>().As<IAbsenceAccountProvider>();
			builder.RegisterType<TextRequestPersister>().As<ITextRequestPersister>();
			builder.RegisterType<ShiftTradeRequestPersonToPermissionValidator>().As<IShiftTradeRequestPersonToPermissionValidator>();
			builder.RegisterType<ShiftExchangeOfferPersister>().As<IShiftExchangeOfferPersister>();
			builder.RegisterType<ShiftTradeRequestPersister>().As<IShiftTradeRequestPersister>();
			builder.RegisterType<OvertimeRequestPersister>().As<IOvertimeRequestPersister>();
			builder.RegisterType<ShiftTradeRequestProvider>().As<IShiftTradeRequestProvider>().SingleInstance();
			builder.RegisterType<RespondToShiftTrade>().As<IRespondToShiftTrade>();
			builder.RegisterType<ShiftTradePeriodViewModelMapper>().As<IShiftTradePeriodViewModelMapper>();
			builder.RegisterType<ShiftTradeRequestMapper>().As<IShiftTradeRequestMapper>();
			builder.RegisterType<PossibleShiftTradePersonsProvider>().As<IPossibleShiftTradePersonsProvider>();
			builder.RegisterType<CreateHourText>().As<ICreateHourText>();
			builder.RegisterType<ShiftTradeTimeLineHoursViewModelFactory>().As<IShiftTradeTimeLineHoursViewModelFactory>();
			builder.RegisterType<ShiftTradeScheduleViewModelMapper>().As<IShiftTradeScheduleViewModelMapper>();
			builder.RegisterType<ShiftTradeAddPersonScheduleViewModelMapper>().As<IShiftTradeAddPersonScheduleViewModelMapper>();
			builder.RegisterType<ShiftTradeTimeLineHoursViewModelMapper>().As<IShiftTradeTimeLineHoursViewModelMapper>();
			builder.RegisterType<ShiftTradeAddScheduleLayerViewModelMapper>().As<IShiftTradeAddScheduleLayerViewModelMapper>();
			builder.RegisterType<RequestsShiftTradeScheduleFilterViewModelFactory>()
				.As<IRequestsShiftTradeScheduleFilterViewModelFactory>()
				.SingleInstance();
			builder.RegisterType<RequestsShiftTradeBulletinViewModelFactory>()
				.As<IRequestsShiftTradeBulletinViewModelFactory>()
				.SingleInstance();
			builder.RegisterType<RequestsShiftTradeScheduleViewModelFactory>()
				.As<IRequestsShiftTradeScheduleViewModelFactory>()
				.SingleInstance();
			builder.RegisterType<ShiftExchangeOffer>().As<IShiftExchangeOffer>().SingleInstance();
			builder.RegisterType<ShiftExchangeOfferMapper>().As<IShiftExchangeOfferMapper>().SingleInstance();
			builder.RegisterType<ShiftTradePersonScheduleProvider>().As<IShiftTradePersonScheduleProvider>().SingleInstance();
			builder.RegisterType<ShiftTradePersonScheduleViewModelMapper>()
				.As<IShiftTradePersonScheduleViewModelMapper>()
				.SingleInstance();
			builder.RegisterType<CancelAbsenceRequestCommandProvider>().As<ICancelAbsenceRequestCommandProvider>();
			builder.RegisterType<AbsenceRequestDetailViewModelFactory>().As<IAbsenceRequestDetailViewModelFactory>();
			builder.RegisterType<ShiftTradeSiteOpenHourFilter>().As<IShiftTradeSiteOpenHourFilter>();

			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
		}

		private void registerMapperTypes(ContainerBuilder builder)
		{
			builder.RegisterType<StudentAvailabilityDayFormMapper>()
				.SingleInstance();
			builder.RegisterType<PreferenceDayInputMapper>().SingleInstance();
			builder.RegisterType<RequestsViewModelMapper>().SingleInstance();
			builder.RegisterType<CommonViewModelMapper>().SingleInstance();
			builder.RegisterType<PreferenceViewModelMapper>().SingleInstance();
			builder.RegisterType<PersonAccountViewModelMapper>().SingleInstance();
			builder.RegisterType<PreferenceDomainDataMapper>().SingleInstance();
			builder.RegisterType<PreferenceDayFeedbackViewModelMapper>().SingleInstance();
			builder.RegisterType<PreferenceDayViewModelMapper>().SingleInstance();
			builder.RegisterType<ShiftTradeSwapDetailViewModelMapper>().SingleInstance();
			builder.RegisterType<StudentAvailabilityDayViewModelMapper>().SingleInstance();
			builder.RegisterType<StudentAvailabilityViewModelMapper>().SingleInstance();
			builder.RegisterType<StudentAvailabilityDayFeedbackViewModelMapper>().SingleInstance();
			builder.RegisterType<TeamScheduleDomainDataMapper>().SingleInstance();
			builder.RegisterType<SettingsMapper>().SingleInstance();
			builder.RegisterType<OvertimeAvailabilityInputMapper>().SingleInstance();
			builder.RegisterType<OvertimeAvailabilityViewModelMapper>().SingleInstance();
			builder.RegisterType<PreferenceAndScheduleDayViewModelMapper>().SingleInstance();
			builder.RegisterType<MonthScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<WeekScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<DayScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<ExtendedPreferenceTemplateMapper>().SingleInstance();
			builder.RegisterType<TextRequestFormMapper>().SingleInstance();
			builder.RegisterType<OvertimeRequestFormMapper>().SingleInstance();
			builder.RegisterType<PreferenceNightRestChecker>().As<IPreferenceNightRestChecker>().SingleInstance();
			builder.RegisterType<ShiftTradeMultiSchedulesSelectableChecker>().As<IShiftTradeMultiSchedulesSelectableChecker>().SingleInstance();
		}

		private static void registerPreferenceTypes(ContainerBuilder builder)
		{
			builder.RegisterType<PreferenceViewModelFactory>().As<IPreferenceViewModelFactory>();
			builder.RegisterType<PreferencePeriodFeedbackViewModelFactory>().As<IPreferencePeriodFeedbackViewModelFactory>();
			builder.RegisterType<PreferenceProvider>().As<IPreferenceProvider>();
			builder.RegisterType<PreferenceOptionsProvider>().As<IPreferenceOptionsProvider>();
			builder.RegisterType<PreferencePersister>().As<IPreferencePersister>();
			builder.RegisterType<MustHaveRestrictionSetter>().As<IMustHaveRestrictionSetter>();
			builder.RegisterType<MustHaveRestrictionProvider>().As<IMustHaveRestrictionProvider>();
			builder.RegisterType<PreferenceFeedbackProvider>().As<IPreferenceFeedbackProvider>().SingleInstance();
			builder.RegisterType<PreferencePeriodFeedbackProvider>().As<IPreferencePeriodFeedbackProvider>().SingleInstance();
			builder.RegisterType<WorkTimeMinMaxRestrictionCreator>().As<IWorkTimeMinMaxRestrictionCreator>();
			builder.RegisterType<EffectiveRestrictionForDisplayCreator>().As<IEffectiveRestrictionForDisplayCreator>();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>();
			builder.RegisterType<RestrictionCombiner>().As<IEffectiveRestrictionCombiner>();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>();
			builder.RegisterType<WorkTimeMinMaxCalculator>().As<IWorkTimeMinMaxCalculator>();
			builder.RegisterType<PersonRuleSetBagProvider>().As<IPersonRuleSetBagProvider>();
			builder.RegisterType<SchedulePeriodTargetDayOffCalculator>()
				.As<ISchedulePeriodTargetDayOffCalculator>()
				.SingleInstance();
			builder.RegisterType<PeriodScheduledAndRestrictionDaysOff>()
				.As<IPeriodScheduledAndRestrictionDaysOff>()
				.SingleInstance();
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().SingleInstance();
			builder.RegisterType<ExtendedPreferencePredicate>().As<IExtendedPreferencePredicate>().SingleInstance();
			builder.RegisterType<PreferenceTemplateProvider>().As<IPreferenceTemplateProvider>();
			builder.RegisterType<PreferenceWeeklyWorkTimeSettingProvider>().As<IPreferenceWeeklyWorkTimeSettingProvider>();
			builder.RegisterType<PreferenceTemplatePersister>().As<IPreferenceTemplatePersister>();
			builder.RegisterType<PersonPreferenceDayOccupationFactory>()
				.As<IPersonPreferenceDayOccupationFactory>()
				.SingleInstance();
		}

		private static void registerStudentAvailabilityTypes(ContainerBuilder builder)
		{
			builder.RegisterType<StudentAvailabilityViewModelFactory>().As<IStudentAvailabilityViewModelFactory>();
			builder.RegisterType<StudentAvailabilityPersister>().As<IStudentAvailabilityPersister>();
			builder.RegisterType<StudentAvailabilityFeedbackProvider>().As<IStudentAvailabilityFeedbackProvider>();
			builder.RegisterType<StudentAvailabilityPeriodFeedbackProvider>().As<IStudentAvailabilityPeriodFeedbackProvider>();
			builder.RegisterType<StudentAvailabilityPeriodFeedbackViewModelFactory>()
				.As<IStudentAvailabilityPeriodFeedbackViewModelFactory>();
		}

		private void registerScheduleTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleViewModelFactory>().As<IScheduleViewModelFactory>();
			builder.RegisterType<ScheduleDayViewModelFactory>().As<IScheduleDayViewModelFactory>();
			builder.RegisterType<HeaderViewModelFactory>().As<IHeaderViewModelFactory>();
			builder.RegisterType<PeriodViewModelFactory>().As<IPeriodViewModelFactory>();
			builder.RegisterType<PeriodSelectionViewModelFactory>().As<IPeriodSelectionViewModelFactory>();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>();
			builder.RegisterType<ExtractBudgetGroupPeriods>().As<IExtractBudgetGroupPeriods>();
			builder.RegisterType<OvertimeAvailabilityPersister>().As<IOvertimeAvailabilityPersister>();
			builder.RegisterType<AbsenceReportPersister>().As<IAbsenceReportPersister>();
			builder.RegisterType<WeekScheduleDomainDataProvider>().As<IWeekScheduleDomainDataProvider>().SingleInstance();
			builder.RegisterType<DayScheduleDomainDataProvider>().As<IDayScheduleDomainDataProvider>().SingleInstance();
			builder.RegisterType<MonthScheduleDomainDataProvider>().As<IMonthScheduleDomainDataProvider>().SingleInstance();
			builder.RegisterType<StaffingPossibilityViewModelFactory>().As<IStaffingPossibilityViewModelFactory>().SingleInstance();
			builder.RegisterType<ScheduleWeekMinMaxTimeCalculator>().As<IScheduleWeekMinMaxTimeCalculator>().SingleInstance();
			builder.RegisterType<ScheduleDayMinMaxTimeCalculator>().As<IScheduleDayMinMaxTimeCalculator>().SingleInstance();
			builder.RegisterType<SiteOpenHourProvider>().As<ISiteOpenHourProvider>().SingleInstance();
			builder.RegisterType<ScheduledSkillOpenHourProvider>().As<IScheduledSkillOpenHourProvider>().SingleInstance();
			builder.RegisterType<IntradayScheduleEdgeTimeCalculator>().As<IIntradayScheduleEdgeTimeCalculator>();
		}

		private void registerTeamScheduleTypes(ContainerBuilder builder)
		{
			builder.RegisterType<TeamSchedulePermissionViewModelFactory>().As<ITeamSchedulePermissionViewModelFactory>();
			builder.RegisterType<TeamSchedulePersonsProvider>().As<ITeamSchedulePersonsProvider>();
			builder.RegisterType<TeamScheduleProjectionForMtwForMtwProvider>().As<ITeamScheduleProjectionForMTWProvider>();
			builder.RegisterType<LayerViewModelMapper>().As<ILayerViewModelMapper>();
			builder.RegisterType<TeamScheduleAgentScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactoryToggle75989Off>().As<ITeamScheduleViewModelFactoryToggle75989Off>();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>();

			builder.RegisterType<TimeLineViewModelFactory>().As<ITimeLineViewModelFactory>();
			builder.RegisterType<TimeLineViewModelFactoryToggle75989Off>().As<ITimeLineViewModelFactoryToggle75989Off>();
			builder.RegisterType<TimeLineViewModelMapperToggle75989Off>().As<ITimeLineViewModelMapperToggle75989Off>();
			builder.RegisterType<AgentScheduleViewModelMapper>().As<IAgentScheduleViewModelMapper>();
			builder.RegisterType<TeamProvider>().As<ITeamProvider>();
			builder.RegisterType<SiteProvider>().As<ISiteProvider>();
			builder.RegisterType<DefaultTeamProvider>().As<IDefaultTeamProvider>();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<PortalViewModelFactory>().As<IPortalViewModelFactory>();
			builder.RegisterType<DatePickerGlobalizationViewModelFactory>().As<IDatePickerGlobalizationViewModelFactory>();
			builder.RegisterType<AgentBadgeWithinPeriodProvider>().As<IAgentBadgeWithinPeriodProvider>();
		}

		private static void registerMyReportTypes(ContainerBuilder builder)
		{
			builder.RegisterType<DailyMetricsForDayQuery>().As<IDailyMetricsForDayQuery>();
			builder.RegisterType<DetailedAdherenceForDayQuery>().As<IDetailedAdherenceForDayQuery>();
			builder.RegisterType<DailyMetricsMapper>().As<IDailyMetricsMapper>();
			builder.RegisterType<DetailedAdherenceMapper>().As<IDetailedAdherenceMapper>();
			builder.RegisterType<MyReportViewModelFactory>().As<IMyReportViewModelFactory>();
			builder.RegisterType<QueueMetricsForDayQuery>().As<IQueueMetricsForDayQuery>();
			builder.RegisterType<QueueMetricsMapper>().As<IQueueMetricsMapper>();
		}

		private static void registerBadgeLeaderBoardReportTypes(ContainerBuilder builder)
		{
			builder.RegisterType<BadgeLeaderBoardReportViewModelFactory>().As<IBadgeLeaderBoardReportViewModelFactory>();
			builder.RegisterType<BadgeLeaderBoardReportOptionFactory>().As<IBadgeLeaderBoardReportOptionFactory>();
			builder.RegisterType<LeaderboardSettingBasedBadgeProvider>().As<ILeaderboardSettingBasedBadgeProvider>();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>();
			builder.RegisterType<DefaultScenarioScheduleProvider>().As<IScheduleProvider>();
			builder.RegisterType<DefaultScenarioForStudentAvailabilityScheduleProvider>().As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();
			builder.RegisterType<LinkProvider>().As<ILinkProvider>();
			builder.RegisterType<SchedulePersonProvider>().As<ISchedulePersonProvider>();
			builder.RegisterType<ScheduleColorProvider>().As<IScheduleColorProvider>();
			builder.RegisterType<AllowanceProvider>().As<IAllowanceProvider>();
			builder.RegisterType<AbsenceTimeProvider>().As<IAbsenceTimeProvider>();
			builder.RegisterType<AbsenceTimeProviderCache>().As<IAbsenceTimeProviderCache>().SingleInstance();
			builder.RegisterType<AbsenceRequestProbabilityProvider>().As<IAbsenceRequestProbabilityProvider>();
			builder.RegisterType<TeamViewModelFactory>().As<ITeamViewModelFactory>();
			builder.RegisterType<SiteViewModelFactory>().As<ISiteViewModelFactory>();
			builder.RegisterType<PersonNameProvider>().As<IPersonNameProvider>().SingleInstance();
			builder.RegisterType<TimeFilterHelper>().As<ITimeFilterHelper>().SingleInstance();
		}
	}
}