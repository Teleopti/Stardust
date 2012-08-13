using System;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfService.Factory;
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
            builder.RegisterType<MessageBrokerEnablerFactory>().As<IMessageBrokerEnablerFactory>();
            builder.RegisterType<InvokeCommand>().AsImplementedInterfaces();
            builder.RegisterType<ScheduleDictionaryModifiedCallback>().As<IScheduleDictionaryModifiedCallback>();
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
