using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Maintainer
{
	[TestFixture]
	[RtaTest]
	public class AgentStateMaintainerTest
	{
		public FakeRtaDatabase Database;
		public AgentStateMaintainer Target;
		public MutableNow Now;
		public FakeAgentStatePersister Persister;

		[Test]
		public void ShouldPrepare()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				TeamId = Guid.NewGuid(),
				ExternalLogons = new[] { new ExternalLogon() }
			});

			Persister.ForPersonId(person).Should().Not.Be.Null();
		}
		
	}
}