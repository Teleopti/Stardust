using System;
using Autofac;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
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
