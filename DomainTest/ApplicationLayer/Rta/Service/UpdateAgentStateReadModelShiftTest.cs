using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
    [TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelShiftTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public IJsonDeserializer Deserializer;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersistShift()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:00");
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00");

			Target.CheckForActivityChanges(Database.TenantName());
			
			var shift = Database.PersistedReadModel.Shift;
			shift.Single().StartTime.Should().Be("2016-05-30 09:00");
			shift.Single().EndTime.Should().Be("2016-05-30 10:00");
			shift.Single().Color.Should().Be("#008000");
		}

		[Test]
		public void ShouldPersistTwoActivities()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:00");
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = Database.PersistedReadModel.Shift;

			shift.Count().Should().Be(2);
			shift.First().Color.Should().Be("#008000");
			shift.First().StartTime.Should().Be("2016-05-30 09:00");
			shift.First().EndTime.Should().Be("2016-05-30 10:00");
			shift.Last().Color.Should().Be("#FF0000");
			shift.Last().StartTime.Should().Be("2016-05-30 10:00");
			shift.Last().EndTime.Should().Be("2016-05-30 11:00");
		}

		[Test]
		public void ShouldExcludeActivityBeforeTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 11:30");
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = Database.PersistedReadModel.Shift;

			shift.Single().Color.Should().Be("#FF0000");
		}

		[Test]
		public void ShouldExcludeActivityAfterTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 06:30");
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = Database.PersistedReadModel.Shift;

			shift.Single().Color.Should().Be("#008000");
		}
	}

}