using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class AgentStateMaintainerTest
	{
		public FakeDatabase Database;
		public AgentStateMaintainer Target;
		public MutableNow Now;
		public FakeAgentStatePersister Persister;

		[Test]
		public void ShouldPrepare()
		{
			var person = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonAssociationChangedEvent
				{
					PersonId = person,
					TeamId = Guid.NewGuid(),
					ExternalLogons = new[] {new ExternalLogon()}
				}
			});

			Persister.ForPersonId(person).Should().Not.Be.Null();
		}
	}
}