using System;
using Autofac;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService
{
	public class SdkCommandHandlersModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(AddAbsenceCommandHandler).Assembly)
				.Where(isHandler)
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();
			builder.RegisterType<MessageBrokerEnablerFactory>().As<IMessageBrokerEnablerFactory>();
		}

		private static bool isHandler(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("CommandHandler", StringComparison.Ordinal);
		}
	}
}
