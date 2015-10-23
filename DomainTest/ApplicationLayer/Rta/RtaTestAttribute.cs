using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
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

			registerFakeDatabase(system, configuration, null);

			system.AddService(this);
		}

		public void SimulateRestartWith(MutableNow now, FakeRtaDatabase database)
		{
			Reset((b, c) =>
			{
				registerFakeDatabase(b, c, database);
				b.UseTestDouble(now).For<INow>();
			});
		}

		private static void registerFakeDatabase(ISystem system, IIocConfiguration configuration, FakeRtaDatabase database)
		{
			if (database != null)
			{
				system.UseTestDouble(database).For<IDatabaseReader, IDatabaseWriter>();

				system.UseTestDouble(database.ApplicationData).For<IApplicationData, ICurrentApplicationData, IDataSourceForTenant>();

				system.UseTestDouble(database.AgentStateReadModelReader).For<IAgentStateReadModelReader>();
				system.UseTestDouble(database.RtaStateGroupRepository).For<IRtaStateGroupRepository>();
				system.UseTestDouble(database.StateGroupActivityAlarmRepository).For<IStateGroupActivityAlarmRepository>();
				system.UseTestDouble(database.Tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants>();

				system.UseTestDouble(database.TeamOutOfAdherenceReadModelPersister).For<ITeamOutOfAdherenceReadModelPersister>();
				system.UseTestDouble(database.SiteOutOfAdherenceReadModelPersister).For<ISiteOutOfAdherenceReadModelPersister>();
				system.UseTestDouble(database.AdherenceDetailsReadModelPersister).For<IAdherenceDetailsReadModelPersister>();
				system.UseTestDouble(database.AdherencePercentageReadModelPersister).For<IAdherencePercentageReadModelPersister>();
			}
			else
			{
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
			}
		}
		
	}
}