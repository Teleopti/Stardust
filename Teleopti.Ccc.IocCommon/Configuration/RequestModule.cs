using Autofac;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RequestModule : Module
	{
		private readonly IocConfiguration _configuration;

		public RequestModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestFactory>().As<IRequestFactory>();

			if (_configuration.Args().IsFatClient)
			{
				builder.RegisterType<PersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			}
			else
			{
				builder.RegisterType<SlimPersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			}

			builder.RegisterType<ConfigurableBusinessRuleProvider>().As<IBusinessRuleProvider>();
			builder.RegisterType<BudgetGroupAllowanceSpecification>().As<IBudgetGroupAllowanceSpecification>();
			builder.RegisterType<AlreadyAbsentSpecification>().As<IAlreadyAbsentSpecification>();
			builder.RegisterType<AbsenceRequestWaitlistProvider>().As<IAbsenceRequestWaitlistProvider>().SingleInstance();
			builder.RegisterType<WriteProtectedScheduleCommandValidator>().As<IWriteProtectedScheduleCommandValidator>().SingleInstance();
			builder.RegisterType<CancelAbsenceRequestCommandValidator>().As<ICancelAbsenceRequestCommandValidator>().SingleInstance();
			builder.RegisterType<CheckingPersonalAccountDaysProvider>().As<ICheckingPersonalAccountDaysProvider>().SingleInstance();
			builder.RegisterType<RequestApprovalServiceFactory>().As<IRequestApprovalServiceFactory>().InstancePerDependency();
			builder.RegisterType<AbsenceRequestValidatorProvider>().As<IAbsenceRequestValidatorProvider>().SingleInstance();
			builder.RegisterType<MultiAbsenceRequestsUpdater>().As<IMultiAbsenceRequestsUpdater>().InstancePerLifetimeScope();
			builder.RegisterType<AbsenceRequestProcessor>().As<IAbsenceRequestProcessor>().SingleInstance();
			builder.RegisterType<SiteOpenHoursSpecification>().As<ISiteOpenHoursSpecification>();
			builder.RegisterType<OvertimeRequestProcessor>().As<IOvertimeRequestProcessor>();

			builder.RegisterType<OvertimeRequestStartTimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestOpenPeriodValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestSiteOpenHourValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestAlreadyHasScheduleValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestStaffingAvailablePeriodValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestMaximumtimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestMaximumContinuousWorkTimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();

			builder.RegisterType<OvertimeRequestContractWorkRulesValidator>().As<IOvertimeRequestContractWorkRulesValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestAvailableSkillsValidator>().As<IOvertimeRequestAvailableSkillsValidator>().SingleInstance();
			builder.RegisterType<FilterRequestsWithDifferentVersion>().As<IFilterRequestsWithDifferentVersion>().SingleInstance();
			builder.RegisterType<OvertimeRequestOpenPeriodMerger>().As<IOvertimeRequestOpenPeriodMerger>().SingleInstance();

			builder.RegisterType<AbsenceRequestStrategyProcessor>().As<IAbsenceRequestStrategyProcessor>().SingleInstance();
			builder.RegisterType<DenyLongQueuedAbsenceRequests>().As<DenyLongQueuedAbsenceRequests>().SingleInstance();
			builder.RegisterType<ArrangeRequestsByProcessOrder>().As<ArrangeRequestsByProcessOrder>().SingleInstance();
			builder.RegisterType<AlreadyAbsentValidator>().As<IAlreadyAbsentValidator>();
			builder.RegisterType<AbsenceRequestWorkflowControlSetValidator>()
				.As<IAbsenceRequestWorkflowControlSetValidator>();
			builder.RegisterType<AbsenceRequestPersonAccountValidator>().As<IAbsenceRequestPersonAccountValidator>();
			builder.RegisterType<SkillCombinationResourceReadModelValidator>().SingleInstance();
			builder.RegisterType<ShiftTradePendingReasonsService>().As<IShiftTradePendingReasonsService>();
			builder.RegisterType<ExpiredRequestValidator>().As<IExpiredRequestValidator>().SingleInstance();

			builder.RegisterType<AbsenceRequestSynchronousValidator>().As<IAbsenceRequestSynchronousValidator>()
				.SingleInstance();
			builder.RegisterType<BusinessRuleConfigProvider>().As<IBusinessRuleConfigProvider>().SingleInstance();
			registerType
				<IBusinessRuleConfigProvider, BusinessRuleConfigProvider, BusinessRuleConfigProviderToggle74889Off>(builder,
					Toggles.MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889);

			builder.RegisterType<FilterOutRequestsHandledByReadmodel>().As<IFilterRequests>().SingleInstance();

			builder.RegisterType<RequestAllowanceProvider>().As<IRequestAllowanceProvider>().SingleInstance();
			builder.RegisterType<ShiftTradeApproveService>().As<IShiftTradeApproveService>().SingleInstance();

			builder.RegisterType<RequestStrategySettingsReader>().As<IRequestStrategySettingsReader>().SingleInstance();
			builder.RegisterType<RequestProcessor>().As<IRequestProcessor>().SingleInstance();
			builder.RegisterType<AbsenceRequestPersister>().As<IAbsenceRequestPersister>();
			builder.RegisterType<AbsenceRequestModelMapper>().SingleInstance();
			builder.RegisterType<NewAbsenceRequestHandler>().SingleInstance();
			builder.RegisterType<ExistingAbsenceRequestHandler>().SingleInstance();

			builder.RegisterType<AbsenceRequestFourteenDaySetting>().As<IAbsenceRequestSetting>().SingleInstance();


			builder.RegisterType<OvertimeRequestUnderStaffingSkillProvider>().As<IOvertimeRequestUnderStaffingSkillProvider>().SingleInstance();


			builder.RegisterType<OvertimeRequestOpenPeriodProvider>().As<IOvertimeRequestOpenPeriodProvider>();
			builder.RegisterType<OvertimeRequestSkillProvider>().As<IOvertimeRequestSkillProvider>();
			builder.RegisterType<SkillStaffingDataSkillTypeFilter>().As<ISkillStaffingDataSkillTypeFilter>();
			builder.RegisterType<OvertimeActivityBelongsToDateProviderToggle74984Off>().As<IOvertimeActivityBelongsToDateProvider>();
			registerType
				<IOvertimeActivityBelongsToDateProvider, OvertimeActivityBelongsToDateProvider, OvertimeActivityBelongsToDateProviderToggle74984Off>(builder,
					Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984);

			builder.RegisterType<SkillOpenHourFilter>().As<ISkillOpenHourFilter>();
			builder.RegisterType<WaitlistPreloadService>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<SmartDeltaDoer>().AsSelf().SingleInstance();

			builder.RegisterType<OvertimeRequestAvailability>().As<IOvertimeRequestAvailability>();
			builder.RegisterType<ShiftTradeRequestAvailability>().As<IShiftTradeRequestAvailability>();
			builder.RegisterType<LicenseAvailability>().As<ILicenseAvailability>();

			registerType
				<IAnyPersonSkillsOpenValidator, AnyPersonSkillsOpenValidatorOptimizationOn, AnyPersonSkillsOpenValidatorOptimizationOff>(builder,
					Toggles.WFM_AbsenceRequest_ImproveThroughput_79139);

			builder.RegisterType<ShiftEndTimeProvider>().As<IShiftEndTimeProvider>().SingleInstance();
			builder.RegisterType<ShiftStartTimeProvider>().As<IShiftStartTimeProvider>().SingleInstance();
			builder.RegisterType<OvertimeRequestDefaultStartTimeProvider>().As<IOvertimeRequestDefaultStartTimeProvider>().SingleInstance();
			builder.RegisterType<AbsenceStaffingPossibilityCalculator>().As<IAbsenceStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<OvertimeStaffingPossibilityCalculator>().As<IOvertimeStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffingIntervalUnderstaffing>().As<ISkillStaffingIntervalUnderstaffing>().SingleInstance();
			builder.RegisterType<BudgetGroupAllowanceCalculatorExtended>().As<IBudgetGroupAllowanceCalculator>()
				.SingleInstance();
			builder.RegisterType<BudgetGroupHeadCountSpecificationExtended>().As<IBudgetGroupHeadCountCalculator>()
				.SingleInstance();

			builder.RegisterType<OvertimeRequestCriticalUnderStaffedSpecification>().As<IOvertimeRequestCriticalUnderStaffedSpecification>()
				.SingleInstance();

			builder.RegisterType<PrimaryPersonSkillFilter>().As<IPrimaryPersonSkillFilter>().SingleInstance();
		}

		private void registerType<T, TToggleOn, TToggleOff>(ContainerBuilder builder, Toggles toggle)
			where TToggleOn : T
			where TToggleOff : T
		{
			if (_configuration.IsToggleEnabled(toggle))
			{
				builder.RegisterType<TToggleOn>().As<T>().SingleInstance();
			}
			else
			{
				builder.RegisterType<TToggleOff>().As<T>().SingleInstance();
			}
		}
	}
}