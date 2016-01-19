using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class ReadModelUpdaterTest : IoCTestAttribute
	{
		protected override FakeToggleManager Toggles()
		{
			var toggles = base.Toggles();
			toggles.Enable(Domain.FeatureFlags.Toggles.RTA_NewEventHangfireRTA_34333);
			return toggles;
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister>();
			system.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister>();
			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister, ITeamOutOfAdherenceReadModelReader>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister, ISiteOutOfAdherenceReadModelReader>();
			system.UseTestDouble<FakeAgentStateReadModelPersister>().For<IAgentStateReadModelPersister>();

			system.UseTestDouble<ControllableReadModelTransactionSyncronization>().For<IReadModelTransactionSyncronization>();
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();

			system.AddService<TeamOutOfAdherenceReadModelUpdater>();
			system.AddService<SiteOutOfAdherenceReadModelUpdater>();
			system.AddService<AdherencePercentageReadModelUpdater>();
			system.AddService<AdherenceDetailsReadModelUpdater>();
			system.AddService<AgentStateReadModelUpdater>();

		}
	}

}