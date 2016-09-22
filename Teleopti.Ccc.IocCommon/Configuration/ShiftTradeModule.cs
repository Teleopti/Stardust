using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ShiftTradeModule : Module
	{
		private readonly IIocConfiguration _configuration;


		public ShiftTradeModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ShiftTradeSkillSpecification>().As<IShiftTradeLightSpecification>();
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<IShiftTradeLightSpecification>();
			builder.RegisterType<ShiftTradeDateSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeTargetTimeSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeAbsenceSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradePersonalActivitySpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeMeetingSpecification>().As<IShiftTradeSpecification>();

			builder.RegisterType<ScheduleProjectionReadOnlyActivityProvider>()
			  .As<IScheduleProjectionReadOnlyActivityProvider>()
			  .SingleInstance();

			if (_configuration.Toggle (Toggles.Wfm_Requests_Check_Max_Seats_39937))
			{
				builder.RegisterType<ShiftTradeMaxSeatsSpecification>().As<IShiftTradeSpecification>();
				builder.RegisterType<ShiftTradeMaxSeatReadModelValidator>().As<IShiftTradeMaxSeatValidator>().SingleInstance();
			}
			else if (_configuration.Toggle (Toggles.Wfm_Requests_Check_Max_Seats_NoReadModel_39937))
			{
				builder.RegisterType<ShiftTradeMaxSeatsSpecification>().As<IShiftTradeSpecification>();
				builder.RegisterType<ShiftTradeMaxSeatValidator>().As<IShiftTradeMaxSeatValidator>().SingleInstance();
			}

			builder.RegisterType<ShiftTradeValidator>().As<IShiftTradeValidator>();
			builder.RegisterType<ShiftTradeLightValidator>().As<IShiftTradeLightValidator>();
			builder.RegisterType<ShiftTradeRequestSetChecksum>().As<IShiftTradeRequestSetChecksum>().SingleInstance();
			builder.RegisterType<ShiftTradeRequestStatusChecker>().As<IShiftTradeRequestStatusChecker>().SingleInstance();
			builder.RegisterType<ShiftTradeSwapScheduleDetailsMapper>().As<IShiftTradeSwapScheduleDetailsMapper>().SingleInstance();
		}
	}
}