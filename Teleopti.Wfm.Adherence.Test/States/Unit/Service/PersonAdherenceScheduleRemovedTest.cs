using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonAdherenceScheduleRemovedTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Rta Target;
		public FakeAgentStatePersister X;

		[Test]
		public void ShouldPublishInAdherenceOnCurrentTimeWhenShiftIsRemoved()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2016-02-23 08:00", "2016-02-23 10:00")
				.WithMappedRule("loggedout", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("loggedout", null, 0, Adherence.Configuration.Adherence.In)
				;

			Now.Is("2016-02-23 08:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedout"
			});

			Database.ClearAssignments(personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2016-02-23 08:05".Utc());
		}
	}
}