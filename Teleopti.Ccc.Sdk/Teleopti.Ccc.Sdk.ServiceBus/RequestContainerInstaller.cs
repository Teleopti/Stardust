using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class RequestContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AbsenceRequestOpenPeriodMerger>().As<IAbsenceRequestOpenPeriodMerger>().SingleInstance();
			builder.RegisterType<RequestFactory>().As<IRequestFactory>().SingleInstance();
			builder.RegisterType<PersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
		}
    }
}