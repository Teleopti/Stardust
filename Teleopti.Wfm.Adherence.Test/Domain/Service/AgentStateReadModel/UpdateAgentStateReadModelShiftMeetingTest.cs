using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Wfm.Adherence.Test.Domain.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelShiftMeetingTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersistMeeting()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var training = Guid.NewGuid();
			Now.Is("2016-12-15 14:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "phone", "2016-12-15 09:00", "2016-12-15 17:00")
				.WithActivity(training, "training")
				.WithMeeting("meeting", "2016-12-15 13:00", "2016-12-15 15:00")
				;

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single(x => x.Name == "training");
			shift.StartTime.Should().Be("2016-12-15 13:00".Utc());
			shift.EndTime.Should().Be("2016-12-15 15:00".Utc());
		}
	}
}