using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.FullTests
{
	[RtaTestLoggedOn]
	[TestFixture]
	[ToggleOff(Toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786)]
	public class InconsistentPercentage32862
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeAdherencePercentageReadModelPersister Percentage;
		public AdherencePercentageViewModelBuilder PercentageView;
		public FakeEventPublisher Publisher;
		
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
				.WithMappedRule("ready", phone, 0, Adherence.In)
				.WithMappedRule("ready", admin, 0, Adherence.Neutral)
				.WithMappedRule(null, phone, 0, Adherence.Out)
				.WithMappedRule("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-03-31 5:00:15");
			Target.ProcessState(new StateForTest
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
				.WithMappedRule("ready", phone, 0, Adherence.In)
				.WithMappedRule("ready", admin, 0, Adherence.Neutral)
				.WithMappedRule(null, phone, 0, Adherence.Out)
				.WithMappedRule("ready", null, 0, Adherence.Out)
				;

			Now.Is("2015-03-31 4:44:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "user",
				StateCode = "ready"
			});
			Now.Is("2015-03-31 4:45:10");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-03-31 5:00:15");
			Target.ProcessState(new StateForTest
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