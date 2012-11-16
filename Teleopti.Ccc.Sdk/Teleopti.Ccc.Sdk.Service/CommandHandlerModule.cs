using System;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService
{
    public class CommandHandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IHandleCommand<>).Assembly)
                .Where(isHandler)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<InvokeCommand>().AsImplementedInterfaces();
        }

        private static bool isHandler(Type infrastructureType)
        {
            return infrastructureType.Name.EndsWith("CommandHandler", StringComparison.Ordinal);
        }
    }

	public class UpdateScheduleModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>().
				InstancePerDependency();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerDependency();
		}
	}
}
