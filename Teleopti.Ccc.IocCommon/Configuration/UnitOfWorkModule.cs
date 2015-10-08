using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class UnitOfWorkModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public UnitOfWorkModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType(_configuration.Args().ImplementationTypeForCurrentUnitOfWork).As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();

			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			builder.RegisterType<DataSourceState>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>().SingleInstance();
			builder.Register(c => c.Resolve<ICurrentDataSource>().Current()).As<IDataSource>().ExternallyOwned();

			builder.RegisterType<CurrentMessageSenders>()
				.As<ICurrentMessageSenders>()
				.As<IMessageSendersScope>()
				.SingleInstance();

			builder.RegisterType<EventsMessageSender>().As<IMessageSender>().SingleInstance();
			if(_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				builder.RegisterType<AggregatedScheduleChangeMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>().SingleInstance()
				.OnActivated(e => CurrentBusinessUnit.SetInstanceFromContainer(e.Instance))
				.OnRelease(e => CurrentBusinessUnit.SetInstanceFromContainer(null))
				;
			builder.RegisterType<HttpRequestFalse>().As<IIsHttpRequest>().SingleInstance();


			// placed here because at the moment uow is the "owner" of the *current* initiator identifier
			builder.RegisterType<CurrentInitiatorIdentifier>().As<ICurrentInitiatorIdentifier>();
			builder.RegisterType<LicenseActivatorProvider>().As<ILicenseActivatorProvider>().SingleInstance();
			builder.RegisterType<CheckLicenseExists>().As<ICheckLicenseExists>().SingleInstance();
			builder.RegisterType<BusinessUnitFilterOverrider>().As<IBusinessUnitFilterOverrider>().SingleInstance();

			// these keep scope state and cant be single instance
			builder.RegisterType<UnitOfWorkAspect>().As<IUnitOfWorkAspect>().InstancePerDependency();
			builder.RegisterType<AllBusinessUnitsUnitOfWorkAspect>().As<IAllBusinessUnitsUnitOfWorkAspect>().InstancePerDependency();
		}
	}
}