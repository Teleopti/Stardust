using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class GlobalSearchTestAttribute : IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));

		    extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
		    isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			isolate.UseTestDouble(new FakeToggleManager(Domain.FeatureFlags.Toggles.Wfm_WebPlan_Pilot_46815)).For<IToggleManager>();
			isolate.UseTestDouble<FakeNextPlanningPeriodProvider>().For<INextPlanningPeriodProvider>();
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
		}
	}
}