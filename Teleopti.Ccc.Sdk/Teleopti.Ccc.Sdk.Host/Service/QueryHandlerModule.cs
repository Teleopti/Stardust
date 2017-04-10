using System;
using Autofac;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
    public class QueryHandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(GetPersonByIdQueryHandler).Assembly)
                .Where(isHandler)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(InvokeQuery<>)).AsImplementedInterfaces();
		}

        private static bool isHandler(Type infrastructureType)
        {
            return infrastructureType.Name.EndsWith("QueryHandler", StringComparison.Ordinal);
        }
    }
}
