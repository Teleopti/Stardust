using System;
using Autofac;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

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
}
