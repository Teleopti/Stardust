using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class MeetingTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Rta Target;

		[Test]
		public void ShouldPersistMeeting()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var training = Guid.NewGuid();
			Now.Is("2016-12-15 14:00");
			Database
				.WithMappedRule("training", training, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("training", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("training", null, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("phone", training, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("phone", null, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule(null, phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule(null, training, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule(null, null, 0, Adherence.Configuration.Adherence.In)
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "phone", "2016-12-15 09:00", "2016-12-15 17:00")
				.WithActivity(training, "training")
				.WithMeeting("meeting", "2016-12-15 13:00", "2016-12-15 15:00")
				;

			Publisher.Clear();
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "training"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single().Timestamp.Should().Be("2016-12-15 14:00".Utc());
		}
	}
}