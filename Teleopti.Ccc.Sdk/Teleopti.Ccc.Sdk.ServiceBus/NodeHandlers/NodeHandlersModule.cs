using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class NodeHandlersModule :Module
	{
		private readonly IocConfiguration _configuration;

		public NodeHandlersModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new SchedulingCommonModule(_configuration));

			//builder.RegisterType<NextPlanningPeriodProvider>()
			//	.SingleInstance()
			//	.As<INextPlanningPeriodProvider>();

			builder.RegisterModule(new RuleSetModule(_configuration, true));

			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkill>>().SingleInstance();
			builder.RegisterType<RunSchedulingHandler>().As<IHandle<ScheduleOnNode>>().SingleInstance().ApplyAspects();
			builder.RegisterType<SchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>();
		}
	}
}