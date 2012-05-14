using System;
using Autofac;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService
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
			builder.RegisterType<ShiftTradeSkillSpecification>().As<ISpecification<IShiftTradeAvailableCheckItem>>();
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<ISpecification<IShiftTradeAvailableCheckItem>>();
        	builder.RegisterType<IsWorkflowControlSetNullSpecification>().As<ISpecification<IShiftTradeAvailableCheckItem>>
					();
		}

        private static bool isHandler(Type infrastructureType)
        {
            return infrastructureType.Name.EndsWith("QueryHandler", StringComparison.Ordinal);
        }
    }
}
