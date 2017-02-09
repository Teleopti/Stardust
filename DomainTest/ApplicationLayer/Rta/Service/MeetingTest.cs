using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class MeetingTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersistMeeting()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var training = Guid.NewGuid();
			Now.Is("2016-12-15 14:00");
			Database
				.WithRule("training", training, 0, Adherence.In)
				.WithRule("training", phone, -1, Adherence.Out)
				.WithRule("training", null, -1, Adherence.Out)
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("phone", training, -1, Adherence.Out)
				.WithRule("phone", null, -1, Adherence.Out)
				.WithRule(null, phone, -1, Adherence.Out)
				.WithRule(null, training, -1, Adherence.Out)
				.WithRule(null, null, 0, Adherence.In)
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "phone", "2016-12-15 09:00", "2016-12-15 17:00")
				.WithActivity(training, "training")
				.WithMeeting("meeting", "2016-12-15 13:00", "2016-12-15 15:00")
				;

			Publisher.Clear();
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "training"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single().Timestamp.Should().Be("2016-12-15 14:00".Utc());
		}
	}
}