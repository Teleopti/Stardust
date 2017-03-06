using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonRuleChangedEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("statecode", null)
				;
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishEventOnlyIfRuleChanged()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)

				.WithStateGroup(null, "state1")
				.WithStateCode("state1")
				.WithRule(null, "rule1", 0, Adherence.Neutral, Color.Black)
				.WithMapping()

				.WithStateGroup(null, "state2")
				.WithStateCode("state2")
				.WithMapping()

				.WithStateGroup(null, "state3")
				.WithStateCode("state3")
				.WithRule(null, "rule3", 0, Adherence.Neutral, Color.Black)
				.WithMapping()
				;

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state2"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state3"
			});

			var events = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

	}

}