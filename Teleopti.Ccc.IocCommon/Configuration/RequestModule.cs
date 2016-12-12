﻿using Autofac;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

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
			if (_configuration.Toggle(Toggles.Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770))
			{
				builder.RegisterType<ConfigurableBusinessRuleProvider>().As<IBusinessRuleProvider>();
			}
			else
			{
				builder.RegisterType<BusinessRuleProvider>().As<IBusinessRuleProvider>();
			}
			builder.RegisterType<BudgetGroupAllowanceSpecification>().As<IBudgetGroupAllowanceSpecification>();
			builder.RegisterType<AlreadyAbsentSpecification>().As<IAlreadyAbsentSpecification>();
			builder.RegisterType<AbsenceRequestUpdater>().As<IAbsenceRequestUpdater>().SingleInstance();
			builder.RegisterType<AbsenceRequestWaitlistProvider>().As<IAbsenceRequestWaitlistProvider>();
			builder.RegisterType<AbsenceRequestWaitlistProcessor>().As<IAbsenceRequestWaitlistProcessor>().SingleInstance();
			builder.RegisterType<AbsenceRequestProcessor>().As<IAbsenceRequestProcessor>().SingleInstance();
			builder.RegisterType<WriteProtectedScheduleCommandValidator>().As<IWriteProtectedScheduleCommandValidator>().SingleInstance();
			builder.RegisterType<CancelAbsenceRequestCommandValidator>().As<ICancelAbsenceRequestCommandValidator>().SingleInstance();
			builder.RegisterType<CheckingPersonalAccountDaysProvider>().As< ICheckingPersonalAccountDaysProvider>().SingleInstance();
			builder.RegisterType<RequestApprovalServiceFactory>().As<IRequestApprovalServiceFactory>().InstancePerDependency();
			builder.RegisterType<AbsenceRequestValidatorProvider>().As<IAbsenceRequestValidatorProvider>().SingleInstance();
			builder.RegisterType<AbsenceRequestIntradayFilter>().As<IAbsenceRequestIntradayFilter>().SingleInstance();
			builder.RegisterType<MultiAbsenceRequestsUpdater>().As<IMultiAbsenceRequestsUpdater>().InstancePerLifetimeScope();			

			registerType<IFilterRequestsWithDifferentVersion, FilterRequestsWithDifferentVersion, FilterRequestsWithDifferentVersion41930ToggleOff>(builder,
				Toggles.Wfm_Requests_ApprovingModifyRequests_41930);

			builder.RegisterType<AbsenceRequestStrategyProcessor>().As<IAbsenceRequestStrategyProcessor>().SingleInstance();
			builder.RegisterType<DenyLongQueuedAbsenceRequests>().As<DenyLongQueuedAbsenceRequests>().SingleInstance();
			builder.RegisterType<ArrangeRequestsByProcessOrder>().As<ArrangeRequestsByProcessOrder>().SingleInstance();
			builder.RegisterType<IntradayRequestProcessor>().As<IIntradayRequestProcessor>().SingleInstance();
			builder.RegisterType<ResourceAllocator>().As<ResourceAllocator>().SingleInstance();
			builder.RegisterType<IntradayRequestWithinOpenHourValidator>().As<IIntradayRequestWithinOpenHourValidator>().SingleInstance();
			builder.RegisterType<AlreadyAbsentValidator>().As<IAlreadyAbsentValidator>();
			builder.RegisterType<AbsenceRequestWorkflowControlSetValidator>()
				.As<IAbsenceRequestWorkflowControlSetValidator>();
			builder.RegisterType<AbsenceRequestPersonAccountValidator>().As<IAbsenceRequestPersonAccountValidator>();

			registerType<IExpiredRequestValidator, ExpiredRequestValidator, ExpiredRequestValidator40274ToggleOff>(builder,
				Toggles.Wfm_Requests_Check_Expired_Requests_40274);
			registerType
				<IAbsenceRequestSynchronousValidator, AbsenceRequestSynchronousValidator,
					AbsenceRequestSynchronousValidator40747ToggleOff>(builder,
						Toggles.MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747);
			registerType
				<IScheduleForecastSkillReadModelValidator, ScheduleForecastSkillReadModelValidator,
					ScheduleForecastSkillReadModelValidator42046ToggleDisabled>(builder,
						Toggles.wfm_Requests_ReadModelOldData_42046);
			registerType
				<IShiftTradePendingReasonsService, ShiftTradePendingReasonsService,
					ShiftTradePendingReasonsService39473ToggleOff>(builder,
						Toggles.Wfm_Requests_Show_Pending_Reasons_39473);
			builder.RegisterType<BusinessRuleConfigProvider>().As<IBusinessRuleConfigProvider>().SingleInstance();
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