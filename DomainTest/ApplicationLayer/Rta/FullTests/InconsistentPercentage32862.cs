using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.FullTests
{
	[RtaTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class InconsistentPercentage32862 : IRegisterInContainer
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public IRta Target;
		public FakeAdherenceDetailsReadModelPersister Details;
		public FakeAdherencePercentageReadModelPersister Percentage;
		public IAdherenceDetailsViewModelBuilder DetailsView;
		public IAdherencePercentageViewModelBuilder PercentageView;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			var detailsPersister = new FakeAdherenceDetailsReadModelPersister();
			var percentagePersister = new FakeAdherencePercentageReadModelPersister();
			var detailsUpdater = new AdherenceDetailsReadModelUpdater(detailsPersister, null, null);
			var percentageUpdater = new AdherencePercentageReadModelUpdater(percentagePersister);
			var publisher = new SyncPublishTo(new object[] {detailsUpdater, percentageUpdater});

			builder.RegisterInstance(detailsPersister).AsImplementedInterfaces().AsSelf();
			builder.RegisterInstance(percentagePersister).AsImplementedInterfaces().AsSelf();
			builder.RegisterInstance(publisher).As<IEventPublisher>().AsSelf();
		}

		[Test]
		public void TestReadModels()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("user", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-03-31 4:45", "2015-03-31 5:00")
				.WithSchedule(personId, admin, "2015-03-31 5:00", "2015-03-31 5:15")
				.WithAlarm("ready", phone, 0, Adherence.In)
				.WithAlarm("ready", admin, 0, Adherence.Neutral)
				.WithAlarm(null, phone, 0, Adherence.Out)
				.WithAlarm("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChange(personId, businessUnitId);
			Now.Is("2015-03-31 5:00:15");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 5:15:28");
			Target.CheckForActivityChange(personId, businessUnitId);

			Details.Details.First().TimeInAdherence.GetValueOrDefault().TotalSeconds.Should().Be(15 * 60);
			Details.Details.First().TimeOutOfAdherence.GetValueOrDefault().TotalSeconds.Should().Be(0);
			Details.Details.Second().TimeInAdherence.GetValueOrDefault().TotalSeconds.Should().Be(0);
			Details.Details.Second().TimeOutOfAdherence.GetValueOrDefault().TotalSeconds.Should().Be(0);
			Details.Model.ActualEndTime.Should().Be(null);
			Percentage.PersistedModel.TimeInAdherence.TotalSeconds.Should().Be(15 * 60);
			Percentage.PersistedModel.TimeOutOfAdherence.TotalSeconds.Should().Be(0);
			Percentage.PersistedModel.ShiftHasEnded.Should().Be(true);
		}

		[Test]
		public void TestViewModels()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("user", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-03-31 4:45", "2015-03-31 5:00")
				.WithSchedule(personId, admin, "2015-03-31 5:00", "2015-03-31 5:15")
				.WithAlarm("ready", phone, 0, Adherence.In)
				.WithAlarm("ready", admin, 0, Adherence.Neutral)
				.WithAlarm(null, phone, 0, Adherence.Out)
				.WithAlarm("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChange(personId, businessUnitId);
			Now.Is("2015-03-31 5:00:15");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 5:15:28");
			Target.CheckForActivityChange(personId, businessUnitId);

			DetailsView.Build(personId).First().AdherencePercent.Should().Be(100);
			PercentageView.Build(personId).AdherencePercent.Should().Be(100);
		}

	}
}