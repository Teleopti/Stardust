using Autofac;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		private readonly IIocConfiguration _configuration;

		public RequestModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestFactory>().As<IRequestFactory>();
			builder.RegisterType<PersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			builder.RegisterType<ConfigurableBusinessRuleProvider>().As<IBusinessRuleProvider>();
			builder.RegisterType<BudgetGroupAllowanceSpecification>().As<IBudgetGroupAllowanceSpecification>();
			builder.RegisterType<AlreadyAbsentSpecification>().As<IAlreadyAbsentSpecification>();
			registerType<IAbsenceRequestWaitlistProvider, AbsenceRequestWaitlistProviderFor46301, AbsenceRequestWaitlistProvider>
				(builder, Toggles.MyTimeWeb_WaitListPositionEnhancement_46301);
			builder.RegisterType<WriteProtectedScheduleCommandValidator>().As<IWriteProtectedScheduleCommandValidator>().SingleInstance();
			builder.RegisterType<CancelAbsenceRequestCommandValidator>().As<ICancelAbsenceRequestCommandValidator>().SingleInstance();
			builder.RegisterType<CheckingPersonalAccountDaysProvider>().As< ICheckingPersonalAccountDaysProvider>().SingleInstance();
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
			builder.RegisterType<OvertimeRequestContractWorkRulesValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestMaximumtimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestMaximumContinuousWorkTimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			
			builder.RegisterType<OvertimeRequestAvailableSkillsValidator>().As<IOvertimeRequestAvailableSkillsValidator>().SingleInstance();
			builder.RegisterType<FilterRequestsWithDifferentVersion>().As<IFilterRequestsWithDifferentVersion>().SingleInstance();

			builder.RegisterType<AbsenceRequestStrategyProcessor>().As<IAbsenceRequestStrategyProcessor>().SingleInstance();
			builder.RegisterType<DenyLongQueuedAbsenceRequests>().As<DenyLongQueuedAbsenceRequests>().SingleInstance();
			builder.RegisterType<ArrangeRequestsByProcessOrder>().As<ArrangeRequestsByProcessOrder>().SingleInstance();
			builder.RegisterType<IntradayRequestWithinOpenHourValidator>().As<IIntradayRequestWithinOpenHourValidator>().SingleInstance();
			builder.RegisterType<AlreadyAbsentValidator>().As<IAlreadyAbsentValidator>();
			builder.RegisterType<AbsenceRequestWorkflowControlSetValidator>()
				.As<IAbsenceRequestWorkflowControlSetValidator>();
			builder.RegisterType<AbsenceRequestPersonAccountValidator>().As<IAbsenceRequestPersonAccountValidator>();
			builder.RegisterType<SkillCombinationResourceReadModelValidator>().SingleInstance();
			builder.RegisterType<ShiftTradePendingReasonsService>().As<IShiftTradePendingReasonsService>();
			builder.RegisterType<ExpiredRequestValidator>().As<IExpiredRequestValidator>().SingleInstance();

			builder.RegisterType<AbsenceRequestSynchronousValidator>().As<IAbsenceRequestSynchronousValidator>()
				.SingleInstance();
			registerType
				<IBusinessRuleConfigProvider, BusinessRuleConfigProvider, BusinessRuleConfigProvider25635ToggleOff>(builder,
						Toggles.Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635);

			registerType
				<IFilterRequests, FilterOutRequestsHandledByReadmodel, NoFilterCheckRequests>(builder,
						Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767);

			
			builder.RegisterType<RequestAllowanceProvider>().As<IRequestAllowanceProvider>().SingleInstance();
			builder.RegisterType<ShiftTradeApproveService>().As<IShiftTradeApproveService>().SingleInstance();

			builder.RegisterType<RequestStrategySettingsReader>().As<IRequestStrategySettingsReader>().SingleInstance();
			registerType
				<IRequestProcessor, RequestProcessor, IntradayRequestProcessorOld>(builder,
						Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767);

			registerType
				<IAbsenceRequestSetting, AbsenceRequestFourteenDaySetting, AbsenceRequestOneDaySetting>(builder,
					Toggles.Wfm_Requests_HandleFourteenDaysFast_43390);

			if (_configuration.Toggle(Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944))
			{
				builder.RegisterType<OvertimeRequestUnderStaffingSkillProviderToggle74944On>().As<IOvertimeRequestUnderStaffingSkillProvider>().SingleInstance();
			}
			else if(_configuration.Toggle(Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853))
			{
				builder.RegisterType<OvertimeRequestUnderStaffingSkillProviderToggle47853On>().As<IOvertimeRequestUnderStaffingSkillProvider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<OvertimeRequestUnderStaffingSkillProvider>().As<IOvertimeRequestUnderStaffingSkillProvider>().SingleInstance();
			}

			builder.RegisterType<SkillStaffingReadModelDataLoader>().As<ISkillStaffingReadModelDataLoader>();
			builder.RegisterType<OvertimeRequestOpenPeriodProvider>().As<IOvertimeRequestOpenPeriodProvider>();
			builder.RegisterType<OvertimeRequestSkillProvider>().As<IOvertimeRequestSkillProvider>();
			builder.RegisterType<SkillStaffingDataSkillTypeFilter>().As<ISkillStaffingDataSkillTypeFilter>();

			builder.RegisterType<SkillOpenHourFilter>().As<ISkillOpenHourFilter>();
			builder.RegisterType<WaitlistPreloadService>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<SmartDeltaDoer>().AsSelf().SingleInstance();

			builder.RegisterType<OvertimeRequestAvailability>().As<IOvertimeRequestAvailability>();
			builder.RegisterType<LicenseAvailability>().As<ILicenseAvailability>();

			registerType
				<IAnyPersonSkillsOpenValidator, AnyPersonSkillsOpenValidator, AnyPersonSkillOpenTrueValidator>(builder,
					Toggles.Wfm_Requests_DenyRequestWhenAllSkillsClosed_46384
				);
			builder.RegisterType<ShiftEndTimeProvider>().As<IShiftEndTimeProvider>().SingleInstance();
			builder.RegisterType<ShiftStartTimeProvider>().As<IShiftStartTimeProvider>().SingleInstance();
			builder.RegisterType<OvertimeRequestDefaultStartTimeProvider>().As<IOvertimeRequestDefaultStartTimeProvider>().SingleInstance();
			builder.RegisterType<AbsenceStaffingPossibilityCalculator>().As<IAbsenceStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<OvertimeStaffingPossibilityCalculator>().As<IOvertimeStaffingPossibilityCalculator>().SingleInstance();

			registerType<IOvertimeRequestCriticalUnderStaffedSpecification,
				OvertimeRequestCriticalUnderStaffedSpecificationToggle74944On, OvertimeRequestCriticalUnderStaffedSpecification>(
				builder, Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944);
		}

		private void registerType<T, TToggleOn, TToggleOff>(ContainerBuilder builder, Toggles toggle)
			where TToggleOn : T
			where TToggleOff : T
		{
			if (_configuration.Toggle(toggle))
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