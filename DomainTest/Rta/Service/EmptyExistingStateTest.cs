using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Rta.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class EmptyExistingStateTest
	{
		public FakeDatabase Database;
		public Domain.Rta.Service.Rta Rta;
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