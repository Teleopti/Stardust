using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.FullTests
{
	[DomainTest]
	[TestFixture]
	public class InconsistentPercentage32862 : ISetup
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeAdherencePercentageReadModelPersister Percentage;
		public IAdherencePercentageViewModelBuilder PercentageView;
		public ConfigurableSyncEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void TestReadModels()
		{
			Publisher.AddHandler(typeof(AdherencePercentageReadModelUpdater));
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
					.WithAgent("user", personId)
					.WithSchedule(personId, phone, "2015-03-31 4:45", "2015-03-31 5:00")
					.WithSchedule(personId, admin, "2015-03-31 5:00", "2015-03-31 5:15")
					.WithRule("ready", phone, 0, Adherence.In)
					.WithRule("ready", admin, 0, Adherence.Neutral)
					.WithRule(null, phone, 0, Adherence.Out)
					.WithRule("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-03-31 5:00:15");
			Target.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 5:15:28");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			
			Percentage.PersistedModel.TimeInAdherence.TotalSeconds.Should().Be(15 * 60);
			Percentage.PersistedModel.TimeOutOfAdherence.TotalSeconds.Should().Be(0);
			Percentage.PersistedModel.ShiftHasEnded.Should().Be(true);
		}

		[Test]
		public void TestViewModels()
		{
			Publisher.AddHandler(typeof(AdherencePercentageReadModelUpdater));
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("user", personId)
				.WithSchedule(personId, phone, "2015-03-31 4:45", "2015-03-31 5:00")
				.WithSchedule(personId, admin, "2015-03-31 5:00", "2015-03-31 5:15")
				.WithRule("ready", phone, 0, Adherence.In)
				.WithRule("ready", admin, 0, Adherence.Neutral)
				.WithRule(null, phone, 0, Adherence.Out)
				.WithRule("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-03-31 5:00:15");
			Target.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 5:15:28");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			
			PercentageView.Build(personId).AdherencePercent.Should().Be(100);
		}

	}
}