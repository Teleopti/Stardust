using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new WebModule(configuration, null));

		    builder.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			builder.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			builder.UseTestDouble(new FakeToggleManager(Domain.FeatureFlags.Toggles.Wfm_ResourcePlanner_32892)).For<IToggleManager>();
			builder.UseTestDouble<FakeNextPlanningPeriodProvider>().For<INextPlanningPeriodProvider>();
			builder.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
		}
	}
}