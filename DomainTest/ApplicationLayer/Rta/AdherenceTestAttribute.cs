using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class AdherenceTestAttribute : IoCTestAttribute
	{
		protected override FakeToggleManager Toggles()
		{
			var toggles = base.Toggles();
			toggles.Enable(Domain.FeatureFlags.Toggles.RTA_NoBroker_31237);
			toggles.Enable(Domain.FeatureFlags.Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285);
			return toggles;
		}

		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister>();
			builder.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister>();
			builder.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
			builder.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister>();

			builder.UseTestDouble<ControllableReadModelTransactionSyncronization>().For<IReadModelTransactionSyncronization>();
			builder.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();

			builder.AddService<TeamOutOfAdherenceReadModelUpdater>();
			builder.AddService<SiteOutOfAdherenceReadModelUpdater>();
			builder.AddService<AdherencePercentageReadModelUpdater>();
			builder.AddService<AdherenceDetailsReadModelUpdater>();

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