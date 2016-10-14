using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[TestFixture]
	public class InitializeProcessTest
	{
		public FakeRtaDatabase Database;
		public FakeAdherencePercentageReadModelPersister Persister;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;
		public ICurrentEventPublisher CurrentEventPublisher;
		public RtaTestAttribute Context;
		
		[Test]
		public void ShouldPublishEventsAfterInitialize()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("user", personId)
				.WithRule("state", Guid.NewGuid())
				.WithRule("anotherstate", Guid.NewGuid());
			Now.Is("2015-01-15 08:00");
			Rta.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});
			EventPublisher.Clear();

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());
			Rta.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "anotherstate"
			});

			EventPublisher.PublishedEvents.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotAddExistingStateCode()
		{
			var personId = Guid.NewGuid();
			Now.Is("2015-10-01 08:00");
			Database
				.WithAgent("user", personId)
				.WithRule("statecode")
				;
			Rta.SaveState(new StateForTest
			{
				UserCode = "user",
				StateCode = "statecode"
			});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

			Database.StateCodes.Should().Have.Count.EqualTo(1);
		}
	}
}