using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class AdherenceTestAttribute : IoCTestAttribute
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
			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister, ISiteOutOfAdherenceReadModelReader>();

			system.UseTestDouble<ControllableReadModelTransactionSyncronization>().For<IReadModelTransactionSyncronization>();
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();

			system.AddService<TeamOutOfAdherenceReadModelUpdater>();
			system.AddService<SiteOutOfAdherenceReadModelUpdater>();
			system.AddService<AdherencePercentageReadModelUpdater>();
			system.AddService<AdherenceDetailsReadModelUpdater>();

			//builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
			//	.OfType<TeamOutOfAdherenceReadModelUpdater>()
			//	.Single()
			//	)
			//	.AsSelf()
			//	.SingleInstance();
			//builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
			//	.OfType<SiteOutOfAdherenceReadModelUpdater>()
			//	.Single()
			//	)
			//	.AsSelf()
			//	.SingleInstance();
			//builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
			//	.OfType<AdherencePercentageReadModelUpdater>()
			//	.Single()
			//	)
			//	.AsSelf()
			//	.SingleInstance();
			//builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonStateChangedEvent>>>()
			//	.OfType<AdherenceDetailsReadModelUpdater>()
			//	.Single()
			//	)
			//	.AsSelf()
			//	.SingleInstance();
		}
	}

}