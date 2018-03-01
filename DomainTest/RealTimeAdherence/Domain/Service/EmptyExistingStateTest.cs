using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Service
{
	[RtaTest]
	[TestFixture]
	public class EmptyExistingStateTest
	{
		public FakeDatabase Database;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Rta;
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