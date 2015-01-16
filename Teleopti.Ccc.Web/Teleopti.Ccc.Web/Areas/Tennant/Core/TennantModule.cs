using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class TennantModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			//ta första appdb för nu
			builder.Register(c =>
			{
				var allDataSources = c.Resolve<IAvailableDataSourcesProvider>().AvailableDataSources();
				return new TennantDatabaseConnectionFactory(allDataSources.First().Application.ConnectionString);
			})
				.As<ITennantDatabaseConnectionFactory>()
				.SingleInstance();
		}
	}
}