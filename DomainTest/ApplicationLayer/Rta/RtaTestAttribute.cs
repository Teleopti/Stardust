using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[LoggedOff]
	public class RtaTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble<FakeRtaDatabase>().For<IDatabaseReader, IDatabaseWriter>();

			system.UseTestDouble<FakeApplicationData>().For<IApplicationData, ICurrentApplicationData, IDataSourceForTenant>();

			system.UseTestDouble<FakeAgentStateReadModelReader>().For<IAgentStateReadModelReader>();
			system.UseTestDouble<FakeRtaStateGroupRepository>().For<IRtaStateGroupRepository>();
			system.UseTestDouble<FakeStateGroupActivityAlarmRepository>().For<IStateGroupActivityAlarmRepository>();
			system.UseTestDouble<FakeTenants>().For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants>();

			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister>();
			system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister>();

			system.AddService(this);
		}
		
	}
}