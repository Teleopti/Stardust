using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class EmptyExistingStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeTeamOutOfAdherenceReadModelPersister Model;
		public RtaTestAttribute Context;
		public FakeEventPublisher Publisher;
		public AgentStateCleaner Handler;

		[Test]
		public void ShouldNotPublishStateChangedEvent()
		{
			var person = Guid.NewGuid();
			var team = Guid.NewGuid();
			Database
				.WithUser("user", person, null, team, null)
				;

			Handler.Handle(new PersonPeriodChangedEvent
			{
				PersonId = person,
				CurrentTeamId = team
			});
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = null,
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotSynchronize()
		{
			var team = Guid.NewGuid();
			Handler.Handle(new PersonPeriodChangedEvent
			{
				PersonId = Guid.NewGuid(),
				CurrentTeamId = team
			});

			Context.SimulateRestart();
			Rta.Touch(Database.TenantName());

			Model.Get(team).Should().Be.Null();
		}

	}
}