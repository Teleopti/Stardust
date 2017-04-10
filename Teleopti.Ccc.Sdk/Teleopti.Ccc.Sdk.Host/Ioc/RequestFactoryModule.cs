using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;

namespace Teleopti.Ccc.Sdk.WcfHost.Ioc
{
	public class RequestFactoryModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ServiceBusSender>()
				.As<IServiceBusSender>()
				.SingleInstance();
			builder.RegisterType<RepositoryFactory>()
				.As<IRepositoryFactory>()
				.SingleInstance();
			builder.RegisterType<PersonRequestFactory>()
				.As<IPersonRequestFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PersistPersonRequest>()
				.As<IPersistPersonRequest>().InstancePerLifetimeScope();

			builder.RegisterType<SdkPersonRequestAuthorizationCheck>().As<IPersonRequestCheckAuthorization>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftTradeRequestStatusChecker>().As<IBatchShiftTradeRequestStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>().InstancePerDependency();
			builder.RegisterType<SwapService>().As<ISwapService>().InstancePerDependency();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>().InstancePerDependency();
		}
	}
}
