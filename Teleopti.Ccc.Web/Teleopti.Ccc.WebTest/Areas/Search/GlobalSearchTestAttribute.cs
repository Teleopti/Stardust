using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class GlobalSearchTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));

			builder.RegisterType<FakeCurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
			builder.RegisterType<FakeUnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();
			builder.RegisterInstance<IToggleManager>(new FakeToggleManager(Domain.FeatureFlags.Toggles.Wfm_ResourcePlanner_32892));
			builder.RegisterType<FakeNextPlanningPeriodProvider>().As<INextPlanningPeriodProvider>();
			builder.RegisterType<FakeApplicationRoleRepository>().As<IApplicationRoleRepository>().SingleInstance();
		}
	}
}