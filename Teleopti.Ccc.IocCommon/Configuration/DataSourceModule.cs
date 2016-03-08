using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class DataSourceModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public DataSourceModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			builder.RegisterType<DataSourceState>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>().SingleInstance();
			builder.Register(c => c.Resolve<ICurrentDataSource>().Current()).As<IDataSource>().ExternallyOwned();

			// license stuff shouldnt be here I think...
			builder.RegisterType<LicenseActivatorProvider>().As<ILicenseActivatorProvider>().SingleInstance();
			builder.RegisterType<CheckLicenseExists>().As<ICheckLicenseExists>().SingleInstance();
		}
	}
}