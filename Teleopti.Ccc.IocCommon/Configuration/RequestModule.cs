﻿using Autofac;
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
			registerType<IBusinessRuleProvider, ConfigurableBusinessRuleProvider, BusinessRuleProvider>(builder,Toggles.Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770);
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
			builder.RegisterType<AbsenceRequestIntradayFilter>().As<IAbsenceRequestIntradayFilter>().SingleInstance();
			builder.RegisterType<SiteOpenHoursSpecification>().As<ISiteOpenHoursSpecification>();
			builder.RegisterType<OvertimeRequestProcessor>().As<IOvertimeRequestProcessor>().SingleInstance();
			builder.RegisterType<OvertimeRequestStartTimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestSiteOpenHourValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestAlreadyHasScheduleValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			builder.RegisterType<OvertimeRequestPeriodValidator>().As<IOvertimeRequestValidator>().SingleInstance();

			if (_configuration.Toggle(Toggles.OvertimeRequestPeriodSetting_46417) && _configuration.Toggle(Toggles.OvertimeRequestPeriodWorkRuleSetting_46638))
				builder.RegisterType<OvertimeRequestContractWorkRulesValidator>().As<IOvertimeRequestValidator>().SingleInstance();

			if (_configuration.Toggle(Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024))
				builder.RegisterType<OvertimeRequestMaximumtimeValidator>().As<IOvertimeRequestValidator>().SingleInstance();
			
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
			
			registerType
				<IAbsenceRequestSynchronousValidator, AbsenceRequestSynchronousValidator,
					AbsenceRequestSynchronousValidator40747ToggleOff>(builder,
						Toggles.MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747);
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

			builder.RegisterType<OvertimeRequestUnderStaffingSkillProvider>().As<IOvertimeRequestUnderStaffingSkillProvider>();
			builder.RegisterType<SkillStaffingReadModelDataLoader>().As<ISkillStaffingReadModelDataLoader>();
			builder.RegisterType<OvertimeRequestSkillProvider>().As<IOvertimeRequestSkillProvider>();
			builder.RegisterType<SkillOpenHourFilter>().As<ISkillOpenHourFilter>();
			builder.RegisterType<WaitlistPreloadService>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<SmartDeltaDoer>().AsSelf().SingleInstance();
			
			registerType
				<IAnyPersonSkillsOpenValidator, AnyPersonSkillsOpenValidator, AnyPersonSkillOpenTrueValidator>(builder,
					Toggles.Wfm_Requests_DenyRequestWhenAllSkillsClosed_46384
				);
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