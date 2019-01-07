using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[RtaTest]
	[TestFixture]
	public class EmptyExistingStateTest
	{
		public FakeDatabase Database;
		public Rta Rta;
		public RtaTestAttribute Context;
		public FakeEventPublisher Publisher;
		
		[Test]
		public void ShouldNotPublishStateChangedEventOnActivityCheck()
		{
			var person = Guid.NewGuid();
			var team = Guid.NewGuid();
			Database
				.WithAgent("user", person, null, team, null)
				;

			Rta.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Be.Empty();
		}

	}
}