using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class RequestContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AbsenceRequestOpenPeriodMerger>().As<IAbsenceRequestOpenPeriodMerger>().SingleInstance();
			builder.RegisterType<RequestFactory>().As<IRequestFactory>().InstancePerDependency();
			builder.RegisterType<PersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			builder.RegisterType<BudgetGroupAllowanceSpecification>().As<IBudgetGroupAllowanceSpecification>();
			builder.RegisterType<AlreadyAbsentSpecification>().As<IAlreadyAbsentSpecification>();
			builder.RegisterType<AbsenceRequestUpdater>().As<IAbsenceRequestUpdater>();
			builder.RegisterType<AbsenceRequestWaitlistProcessor>().As<IAbsenceRequestWaitlistProcessor>();
			builder.RegisterType<AbsenceRequestProcessor>().As<IAbsenceRequestProcessor>();
		}
    }
}