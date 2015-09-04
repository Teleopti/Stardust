using System;
using Autofac;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

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
		}

		private static bool isHandler(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("CommandHandler", StringComparison.Ordinal);
		}
	}
}
