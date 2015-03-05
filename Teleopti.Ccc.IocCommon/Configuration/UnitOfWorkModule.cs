using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class UnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentUnitOfWork>().As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();

			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			builder.Register(c => c.Resolve<ICurrentDataSource>().Current())
				.As<IDataSource>()
				.ExternallyOwned();

			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>();

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