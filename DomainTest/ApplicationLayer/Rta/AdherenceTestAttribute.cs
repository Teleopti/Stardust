using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
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

		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterType<FakeAdherencePercentageReadModelPersister>()
				.As<IAdherencePercentageReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeAdherenceDetailsReadModelPersister>()
				.As<IAdherenceDetailsReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeTeamOutOfAdherenceReadModelPersister>()
				.As<ITeamOutOfAdherenceReadModelPersister>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<FakeSiteOutOfAdherenceReadModelPersister>()
				.As<ISiteOutOfAdherenceReadModelPersister>()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<ControllableLiteTransactionSyncronization>().As<ILiteTransactionSyncronization>().SingleInstance();
			builder.RegisterType<FakeReadModelUnitOfWorkAspect>().As<IReadModelUnitOfWorkAspect>();

			builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
				.OfType<TeamOutOfAdherenceReadModelUpdater>()
				.Single()
				)
				.AsSelf()
				.SingleInstance();
			builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
				.OfType<SiteOutOfAdherenceReadModelUpdater>()
				.Single()
				)
				.AsSelf()
				.SingleInstance();
			builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonInAdherenceEvent>>>()
				.OfType<AdherencePercentageReadModelUpdater>()
				.Single()
				)
				.AsSelf()
				.SingleInstance();
			builder.Register(x => x.Resolve<IEnumerable<IHandleEvent<PersonStateChangedEvent>>>()
				.OfType<AdherenceDetailsReadModelUpdater>()
				.Single()
				)
				.AsSelf()
				.SingleInstance();
		}
	}

}